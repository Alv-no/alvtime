using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.Overtime;
using AlvTime.Business.Payouts;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.Users;
using AlvTime.Business.Utils;
using Microsoft.Extensions.Options;

namespace AlvTime.Business.TimeRegistration;

public class TimeRegistrationService
{
    private readonly IUserContext _userContext;
    private readonly TaskUtils _taskUtils;
    private readonly ITimeRegistrationStorage _timeRegistrationStorage;
    private readonly IDbContextScope _dbContextScope;
    private readonly IPayoutStorage _payoutStorage;
    private readonly UserService _userService;
    private readonly int _flexTask;
    private readonly int _paidHolidayTask;
    private const decimal HoursInWorkday = 7.5M;

    public TimeRegistrationService(
        IOptionsMonitor<TimeEntryOptions> timeEntryOptions,
        IUserContext userContext,
        TaskUtils taskUtils,
        ITimeRegistrationStorage timeRegistrationStorage,
        IDbContextScope dbContextScope,
        IPayoutStorage payoutStorage,
        UserService userService)
    {
        _userContext = userContext;
        _taskUtils = taskUtils;
        _timeRegistrationStorage = timeRegistrationStorage;
        _dbContextScope = dbContextScope;
        _payoutStorage = payoutStorage;
        _userService = userService;
        _flexTask = timeEntryOptions.CurrentValue.FlexTask;
        _paidHolidayTask = timeEntryOptions.CurrentValue.PaidHolidayTask;
    }
    
    public async Task<IEnumerable<TimeEntryResponseDto>> UpsertTimeEntry(IEnumerable<CreateTimeEntryDto> timeEntries)
    {
        var currentUser = await _userContext.GetCurrentUser();
        var userId = currentUser.Id;

        List<TimeEntryResponseDto> response = new List<TimeEntryResponseDto>();

        foreach (var timeEntry in timeEntries)
        {
            var criterias = new TimeEntryQuerySearch
            {
                UserId = userId,
                FromDateInclusive = timeEntry.Date.Date,
                ToDateInclusive = timeEntry.Date.Date,
                TaskId = timeEntry.TaskId
            };

            await ValidateTimeEntry(timeEntry);

            if (await GetTimeEntry(criterias) == null)
            {
                await _dbContextScope.AsAtomic(async () =>
                {
                    var createdTimeEntry = await _timeRegistrationStorage.CreateTimeEntry(timeEntry, userId);
                    response.Add(createdTimeEntry);
                    var entriesOnDay = await GetEntriesWithCompRatesForUserOnDay(userId, timeEntry);
                    await UpdateEarnedOvertime(entriesOnDay);
                });
            }
            else
            {
                await _dbContextScope.AsAtomic(async () =>
                {
                    var updatedTimeEntry = await _timeRegistrationStorage.UpdateTimeEntry(timeEntry, userId);
                    response.Add(updatedTimeEntry);
                    var entriesOnDay = await GetEntriesWithCompRatesForUserOnDay(userId, timeEntry);
                    await UpdateEarnedOvertime(entriesOnDay);
                });
            }
        }

        return response;
    }

    private async Task ValidateTimeEntry(CreateTimeEntryDto timeEntry)
    {
        var currentUser = await _userContext.GetCurrentUser();
        var timeEntriesOnDate = (await _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            FromDateInclusive = timeEntry.Date.Date,
            ToDateInclusive = timeEntry.Date.Date,
            UserId = currentUser.Id
        })).ToDictionary(entry => entry.TaskId, entry => entry);
            
        var latestPayoutDate = (await _payoutStorage
            .GetRegisteredPayouts(new PayoutQueryFilter {UserId = currentUser.Id})).Entries.MaxBy(po => po.Date)?.Date.Date;
            
        var allRedDays = new RedDays(timeEntry.Date.Year).Dates;

        var usersEmploymentRate = await _userService.GetCurrentEmploymentRateForUser(currentUser.Id, timeEntry.Date);
        var anticipatedWorkHours =
            IsWeekend(timeEntry.Date.Date) || allRedDays.Contains(timeEntry.Date.Date) ? 0M : HoursInWorkday * usersEmploymentRate;
            
        if (PayoutWouldBeAffectedByRegistration(timeEntry, latestPayoutDate, timeEntriesOnDate.Values,
                anticipatedWorkHours))
        {
            throw new Exception(
                "Du har registrert en utbetaling som vil bli påvirket av denne timeføringen. Kontakt en admin for å få endret timene dine.");
        }

        timeEntriesOnDate[timeEntry.TaskId] = new TimeEntryResponseDto
        {
            Date = timeEntry.Date,
            Value = timeEntry.Value,
            TaskId = timeEntry.TaskId
        };

        if (allRedDays.Contains((timeEntry.Date.Date)) && timeEntry.TaskId == _paidHolidayTask)
        {
            throw new Exception("Du trenger ikke registrere fravær på en rød dag.");
        }
            
        if (timeEntriesOnDate.Values.Sum(te => te.Value) > anticipatedWorkHours &&
            timeEntriesOnDate.Values.Any(te => te.TaskId == _flexTask && te.Value > 0))
        {
            throw new Exception($"Du kan ikke registrere mer enn {anticipatedWorkHours:0.00} timer når du avspaserer.");
        }

        if (await OvertimeOrFlexingAffectsFutureFlex(timeEntry, anticipatedWorkHours))
        {
            throw new Exception("Fjern fremtidig registrert avspasering før du fører overtid eller avspasering på en tidligere dato.");
        }

        if (PayoutWouldBeAffectedByRegistration(timeEntry, latestPayoutDate, timeEntriesOnDate.Values,
                anticipatedWorkHours))
        {
            throw new Exception(
                "Du har registrert en utbetaling som vil bli påvirket av denne timeføringen. Kontakt en admin for å få endret timene dine.");
        }

        if (timeEntry.TaskId == _flexTask)
        {
            if (latestPayoutDate != null && timeEntry.Date.Date <= latestPayoutDate)
            {
                throw new Exception(
                    "Du har registrert en utbetaling som vil bli påvirket av denne timeføringen. Kontakt en admin for å få endret timene dine.");
            }

            var availableHours = await GetAvailableOvertimeHoursAtDate(timeEntry.Date.Date);
            var availableForFlex = availableHours.AvailableHoursBeforeCompensation;

            if (timeEntry.Value > availableForFlex)
            {
                throw new Exception("Ikke nok tilgjengelige timer til å avspasere.");
            }
        }

        var taskGivesOvertime = await _taskUtils.TaskGivesOvertime(timeEntry.TaskId);
        if (timeEntry.Value > HoursInWorkday && !taskGivesOvertime)
        {
            throw new Exception("Du kan ikke registrere mer enn 7.5 timer på den oppgaven.");
        }

        if (!taskGivesOvertime &&
            (timeEntry.Date.DayOfWeek == DayOfWeek.Saturday || timeEntry.Date.DayOfWeek == DayOfWeek.Sunday))
        {
            throw new Exception("Du kan ikke registrere den oppgaven på en helg.");
        }
    }

    private async Task<bool> OvertimeOrFlexingAffectsFutureFlex(CreateTimeEntryDto timeEntry, decimal anticipatedWorkHours)
    {
        var futureFlexEntries = await _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            UserId = (await _userContext.GetCurrentUser()).Id, FromDateInclusive = timeEntry.Date.Date.AddDays(1), TaskId = _flexTask
        });

        if (futureFlexEntries.Any(e => e.Value > 0) && (timeEntry.TaskId == _flexTask || timeEntry.Value > anticipatedWorkHours))
        {
            return true;
        }

        return false;
    }

    private static bool PayoutWouldBeAffectedByRegistration(CreateTimeEntryDto timeEntry,
        DateTime? latestPayoutDate, IEnumerable<TimeEntryResponseDto> timeEntriesOnDate,
        decimal anticipatedWorkHours)
    {
        var sumOnDate = timeEntriesOnDate.Sum(te => te.Value);
        if (latestPayoutDate != null && sumOnDate > anticipatedWorkHours && timeEntry.Date.Date <= latestPayoutDate)
        {
            return true;
        }
        var existingRegistrationOnTask = timeEntriesOnDate.FirstOrDefault(te => te.TaskId == timeEntry.TaskId);
        if (existingRegistrationOnTask != null)
        {
            sumOnDate -= existingRegistrationOnTask.Value;
        }
        return latestPayoutDate != null && timeEntry.Date.Date <= latestPayoutDate &&
               sumOnDate + timeEntry.Value > anticipatedWorkHours;
    }

    private async Task<List<TimeEntryWithCompRateDto>> GetEntriesWithCompRatesForUserOnDay(int userId,
        CreateTimeEntryDto timeEntry)
    {
        var entriesOnDay = (await _timeRegistrationStorage.GetTimeEntriesWithCompensationRate(
            new TimeEntryQuerySearch
            {
                UserId = userId,
                FromDateInclusive = timeEntry.Date.Date,
                ToDateInclusive = timeEntry.Date.Date
            })).ToList();
        return entriesOnDay;
    }

    private async Task<TimeEntryResponseDto> GetTimeEntry(TimeEntryQuerySearch criterias)
    {
        return await _timeRegistrationStorage.GetTimeEntry(criterias);
    }

    public async Task<List<TimeEntryResponseDto>> GetTimeEntries(TimeEntryQuerySearch criterias)
    {
        var currentUser = await _userContext.GetCurrentUser();
        criterias.UserId = currentUser.Id;
        return (await _timeRegistrationStorage.GetTimeEntries(criterias)).ToList();
    }
        
    public async Task<List<TimeEntryResponseDto>> GetFlexTimeEntries()
    {
        var currentUser = await _userContext.GetCurrentUser();
        var criterias = new TimeEntryQuerySearch { UserId = currentUser.Id, TaskId = _flexTask };
        return (await _timeRegistrationStorage.GetTimeEntries(criterias)).ToList();
    }

    public async Task<List<EarnedOvertimeDto>> GetEarnedOvertime(OvertimeQueryFilter criterias)
    {
        var currentUser = await _userContext.GetCurrentUser();
        criterias.UserId = currentUser.Id;
        return await _timeRegistrationStorage.GetEarnedOvertime(criterias);
    }

    public async Task<AvailableOvertimeDto> GetAvailableOvertimeHoursNow()
    {
        return await GetAvailableOvertimeHoursAtDate(DateTime.Now);
    }

    public async Task<AvailableOvertimeDto> GetAvailableOvertimeHoursAtDate(DateTime toDateInclusive)
    {
        var currentUser = await _userContext.GetCurrentUser();

        var earnedOvertime = await _timeRegistrationStorage.GetEarnedOvertime(new OvertimeQueryFilter
        {
            UserId = currentUser.Id,
            EndDate = toDateInclusive.Date
        });

        var overtimeEntries = new List<TimeEntry>();
        overtimeEntries.AddRange(earnedOvertime.Select(eo => new TimeEntry
        {
            Date = eo.Date,
            Hours = eo.Value,
            CompensationRate = eo.CompensationRate
        }));

        await CompensateForPayouts(overtimeEntries, toDateInclusive);
        await CompensateForFlexedHours(overtimeEntries, toDateInclusive);

        var availableBeforeCompRate = overtimeEntries.Sum(e => e.Hours);
        var availableAfterCompRate = overtimeEntries.Sum(e => e.Hours * e.CompensationRate);

        return new AvailableOvertimeDto
        {
            AvailableHoursBeforeCompensation = availableBeforeCompRate,
            AvailableHoursAfterCompensation = availableAfterCompRate,
            Entries = overtimeEntries
        };
    }

    private async Task CompensateForFlexedHours(List<TimeEntry> timeEntries, DateTime toDateInclusive)
    {
        var currentUser = await _userContext.GetCurrentUser();

        var flexedTimeEntries = (await _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            UserId = currentUser.Id,
            TaskId = _flexTask,
            FromDateInclusive = currentUser.StartDate.Date,
            ToDateInclusive = toDateInclusive.Date
        })).ToList();

        foreach (var flexTimeEntry in flexedTimeEntries)
        {
            var sumFlexedHours = flexTimeEntry.Value;

            var relevantOvertimeGroupedByCompRate = timeEntries
                .Where(ot => ot.Date < flexTimeEntry.Date)
                .GroupBy(eo => eo.CompensationRate)
                .Select(eo => new
                {
                    CompensationRate = eo.Key,
                    Hours = eo.Sum(entry => entry.Hours)
                }).OrderBy(e => e.CompensationRate);

            foreach (var overtimeGroup in relevantOvertimeGroupedByCompRate)
            {
                if (sumFlexedHours <= 0)
                {
                    break;
                }

                var entry = new TimeEntry
                {
                    Hours = -Math.Min(sumFlexedHours, overtimeGroup.Hours),
                    CompensationRate = overtimeGroup.CompensationRate,
                    Date = flexTimeEntry.Date
                };

                timeEntries.Add(entry);
                sumFlexedHours += entry.Hours;
            }
        }
    }

    private async Task CompensateForPayouts(List<TimeEntry> overtimeEntries, DateTime toDateInclusive)
    {
        var currentUser = await _userContext.GetCurrentUser();

        var registeredPayouts = await _payoutStorage.GetRegisteredPayouts(new PayoutQueryFilter
        {
            UserId = currentUser.Id,
            ToDateInclusive = toDateInclusive.Date
        });

        var payoutEntriesGroupedByDateAndRate = registeredPayouts.Entries.GroupBy(e => (e.Date, e.CompRate)).OrderBy(g => g.Key.Date);

        foreach (var payoutEntryGroup in payoutEntriesGroupedByDateAndRate)
        {
            TimeEntry payoutEntry = new TimeEntry
            {
                Hours = -payoutEntryGroup.Sum(po => po.HoursBeforeCompRate),
                CompensationRate = payoutEntryGroup.Key.CompRate,
                Date = payoutEntryGroup.Key.Date
            };

            overtimeEntries.Add(payoutEntry);
        }
    }

    private async Task UpdateEarnedOvertime(List<TimeEntryWithCompRateDto> timeEntriesOnDay)
    {
        var currentUser = await _userContext.GetCurrentUser();
        var timeEntryDate = timeEntriesOnDay.First().Date.Date;
        await _timeRegistrationStorage.DeleteOvertimeOnDate(timeEntryDate, currentUser.Id);
        await StoreNewOvertime(timeEntriesOnDay);
    }

    private async Task StoreNewOvertime(List<TimeEntryWithCompRateDto> timeEntriesOnDay)
    {
        var currentUser = await _userContext.GetCurrentUser();

        var timeEntryDate = timeEntriesOnDay.First().Date.Date;
        var allRedDays = new RedDays(timeEntryDate.Year).Dates;
        
        var usersEmploymentRate = await _userService.GetCurrentEmploymentRateForUser(currentUser.Id, timeEntryDate);
        var anticipatedWorkHours =
            IsWeekend(timeEntryDate) || allRedDays.Contains(timeEntryDate) ? 0M : HoursInWorkday * usersEmploymentRate;

        var normalWorkHoursLeft = anticipatedWorkHours;
        
        var overtimeEntries = new List<OvertimeEntry>();

        var imposedTaskIds = await _taskUtils.GetAllImposedTaskIds();
        var orderedTimeEntries = timeEntriesOnDay.Where(te => !imposedTaskIds.Contains(te.TaskId))
            .OrderByDescending(entry => entry.CompensationRate).ToList();
        var imposedTimeEntries = timeEntriesOnDay.Where(te => imposedTaskIds.Contains(te.TaskId)).ToList();
            
        orderedTimeEntries.AddRange(imposedTimeEntries); //Imposed overtime should always be calculated last
            
        foreach (var timeEntry in orderedTimeEntries)
        {
            var taskGivesOvertime = await _taskUtils.TaskGivesOvertime(timeEntry.TaskId);
            if (anticipatedWorkHours == 0 &&
                !taskGivesOvertime) //Guard against absence overtime on red day
            {
                continue;
            }

            if (normalWorkHoursLeft > 0 && normalWorkHoursLeft - timeEntry.Value < 0) //Split entry
            {
                overtimeEntries.Add(new OvertimeEntry
                {
                    Date = timeEntryDate,
                    Hours = timeEntry.Value - normalWorkHoursLeft,
                    CompensationRate = timeEntry.CompensationRate,
                    TaskId = timeEntry.TaskId
                });
            }
            else if (normalWorkHoursLeft <= 0)
            {
                overtimeEntries.Add(new OvertimeEntry
                {
                    Date = timeEntryDate,
                    Hours = timeEntry.Value,
                    CompensationRate = timeEntry.CompensationRate,
                    TaskId = timeEntry.TaskId
                });
            }

            normalWorkHoursLeft -= timeEntry.Value;
        }

        await _timeRegistrationStorage.StoreOvertime(overtimeEntries, currentUser.Id);
    }

    private static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    public async Task RetriggerDate(DateTime date, int userId)
    {
        var firstEntryOnDate = (await _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            UserId = userId,
            FromDateInclusive = date,
            ToDateInclusive = date
        })).First();

        var dto = new CreateTimeEntryDto
        {
            Date = firstEntryOnDate.Date,
            Value = firstEntryOnDate.Value,
            TaskId = firstEntryOnDate.TaskId
        };
        await UpsertTimeEntry(new List<CreateTimeEntryDto> { dto });
    }
}