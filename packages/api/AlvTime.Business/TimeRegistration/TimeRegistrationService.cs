using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Absence;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.Overtime;
using AlvTime.Business.Payouts;
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
    
    public async Task<IEnumerable<TimeEntryOverviewDto>> FetchTimeEntryOverview(int numberOfMonths = 3)
    {
        var user = await _userContext.GetCurrentUser();
        var userId = user.Id;

        var today = DateTime.Today;
        var fromDate = new DateTime(today.Year, today.Month, 1).AddMonths(-numberOfMonths);
        var toDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);

        var timeEntries = await _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            UserId = userId,
            FromDateInclusive = fromDate,
            ToDateInclusive = toDate,
        });

        var monthGroups = timeEntries.GroupBy(te => new { te.Date.Year, te.Date.Month });

        var response = new List<TimeEntryOverviewDto>();
        foreach (var timeEntryGroup in monthGroups)
        {
            var taskGroups = timeEntryGroup.GroupBy(te => te.TaskId);

            var tasksWithHours = new List<TaskWithHours>();
            foreach (var taskGroup in taskGroups)
            {
                tasksWithHours.Add(new TaskWithHours
                {
                    TaskId = taskGroup.Key,
                    Hours = taskGroup.Sum(te => te.Value)
                });
            }

            response.Add(new TimeEntryOverviewDto
            {
                Year = timeEntryGroup.Key.Year,
                Month = timeEntryGroup.Key.Month,
                TasksWithHours = tasksWithHours
            });
        }

        return response;
    }

    public async Task<Result<IEnumerable<TimeEntryResponseDto>>> UpsertTimeEntry(IEnumerable<CreateTimeEntryDto> timeEntries)
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

            var validationErrors = await ValidateTimeEntry(timeEntry);
            if (validationErrors.Any())
            {
                if (string.IsNullOrWhiteSpace(timeEntry.Comment)) return validationErrors;
                
                //Update comment even if validation fails
                var hour = await GetTimeEntry(criterias);
                if (hour == null && timeEntry.Value == 0)
                {
                    await _timeRegistrationStorage.CreateTimeEntry(timeEntry, userId);
                }
                if (hour != null)
                {
                    await _timeRegistrationStorage.UpdateComment(timeEntry.Comment, hour!.Id);
                }

                return validationErrors;
            }

            if (await GetTimeEntry(criterias) == null)
            {
                await _dbContextScope.AsAtomic(async () =>
                {
                    var createdTimeEntry = await _timeRegistrationStorage.CreateTimeEntry(timeEntry, userId);
                    response.Add(createdTimeEntry);
                    var entriesOnDay = await GetEntriesWithCompRatesForUserOnDay(userId, timeEntry.Date);
                    await UpdateEarnedOvertime(entriesOnDay, userId);
                    await UpdateRegisteredFlex(entriesOnDay, userId);
                    await UpdateFutureFlex(entriesOnDay.First().Date, userId);
                });
            }
            else
            {
                await _dbContextScope.AsAtomic(async () =>
                {
                    var updatedTimeEntry = await _timeRegistrationStorage.UpdateTimeEntry(timeEntry, userId);
                    response.Add(updatedTimeEntry);
                    var entriesOnDay = await GetEntriesWithCompRatesForUserOnDay(userId, timeEntry.Date);
                    await UpdateEarnedOvertime(entriesOnDay, userId);
                    await UpdateRegisteredFlex(entriesOnDay, userId);
                    await UpdateFutureFlex(entriesOnDay.First().Date, userId);
                });
            }
        }

        return response;
    }

    private async Task UpdateRegisteredFlex(List<TimeEntryWithCompRateDto> entriesOnDay, int userId)
    {
        if (entriesOnDay.Any(e => e.TaskId == _flexTask))
        {
            var date = entriesOnDay.First().Date.Date;
            await _timeRegistrationStorage.DeleteFlexOnDate(date, userId);
            var amountToFlex = entriesOnDay.Where(e => e.TaskId == _flexTask).Sum(f => f.Value);
            var availableOvertime = await GetAvailableOvertimeHoursAtDate(date);
            var nonImposedOverTime = availableOvertime.Entries
                .Where(ot => ot.Date <= date && ot.CompensationRate < 2.0M)
                .GroupBy(eo => eo.CompensationRate)
                .Select(eo => new
                {
                    CompensationRate = eo.Key,
                    Hours = eo.Sum(entry => entry.Hours)
                }).OrderByDescending(e => e.CompensationRate);

            var imposedOverTime = availableOvertime.Entries.Where(ot => ot.Date <= date && ot.CompensationRate >= 2.0M)
                .GroupBy(eo => eo.CompensationRate)
                .Select(eo => new
                {
                    CompensationRate = eo.Key,
                    Hours = eo.Sum(entry => entry.Hours)
                }).OrderByDescending(e => e.CompensationRate);

            var overtimeGroupedByCompRate = nonImposedOverTime.Concat(imposedOverTime).ToList(); //Imposed overtime should be subtracted last

            foreach (var overtimeGroup in overtimeGroupedByCompRate)
            {
                if (amountToFlex <= 0)
                {
                    break;
                }

                var amountToFlexOnCompRate = Math.Min(amountToFlex, overtimeGroup.Hours);
                await _timeRegistrationStorage.RegisterFlex(new TimeEntry
                {
                    Date = date,
                    CompensationRate = overtimeGroup.CompensationRate,
                    Hours = amountToFlexOnCompRate
                }, userId);

                amountToFlex -= amountToFlexOnCompRate;
            }
        }
    }

    private async Task UpdateFutureFlex(DateTime date, int userId)
    {
        var futureFlexEntries = (await _timeRegistrationStorage.GetFlexEntries(new TimeEntryQuerySearch
        {
            UserId = userId,
            FromDateInclusive = date.AddDays(1)
        })).ToList();

        if (futureFlexEntries.Any())
        {
            var orderedFlexEntries = futureFlexEntries.OrderBy(f => f.Date);
            foreach (var futureFlex in orderedFlexEntries)
            {
                var entriesOnDay = await GetEntriesWithCompRatesForUserOnDay(userId, futureFlex.Date);
                await UpdateRegisteredFlex(entriesOnDay, userId);
            }
        }
    }

    private async Task<List<Error>> ValidateTimeEntry(CreateTimeEntryDto timeEntry)
    {
        var timeEntryDate = timeEntry.Date.Date;
        var currentUser = await _userContext.GetCurrentUser();
        var timeEntriesOnDate = (await _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            FromDateInclusive = timeEntryDate,
            ToDateInclusive = timeEntryDate,
            UserId = currentUser.Id
        })).ToDictionary(entry => entry.TaskId, entry => entry);

        var latestPayoutDate = (await _payoutStorage.GetRegisteredPayouts(new PayoutQueryFilter { UserId = currentUser.Id })).Entries.MaxBy(po => po.Date)?.Date.Date;

        var allRedDays = new RedDays(timeEntryDate.Year).Dates;

        var usersEmploymentRateResult = await _userService.GetCurrentEmploymentRateForUser(currentUser.Id, timeEntryDate);
        if (!usersEmploymentRateResult.IsSuccess)
        {
            return usersEmploymentRateResult.Errors;
        }

        var anticipatedWorkHours =
            IsWeekend(timeEntryDate) || allRedDays.Contains(timeEntryDate)
                ? 0M
                : HoursInWorkday * usersEmploymentRateResult.Value;

        if ((IsWeekend(timeEntryDate) || allRedDays.Contains(timeEntryDate)) && (timeEntry.TaskId == _paidHolidayTask || timeEntry.TaskId == _flexTask) && timeEntry.Value > 0)
        {
            return [new Error(ErrorCodes.InvalidAction, "Du trenger ikke registrere fravær på en rød dag.")];
        }
        
        if (timeEntry.TaskId == _flexTask)
        {
            var availableHours = await GetAvailableOvertimeHoursAtDate(timeEntryDate);
            var availableForFlex = availableHours.AvailableHoursBeforeCompensation;

            if (timeEntriesOnDate.Values.Any(te => te.TaskId == _flexTask))
            {
                availableForFlex +=
                    timeEntriesOnDate.First(te => te.Value.TaskId == _flexTask).Value.Value;
            }

            if (timeEntry.Value > availableForFlex)
            {
                return new List<Error> { new(ErrorCodes.InvalidAction, "Ikke nok tilgjengelige timer til å avspasere.") };
            }
        }

        if (PayoutWouldBeAffectedByRegistration(timeEntry, latestPayoutDate, timeEntriesOnDate.Values, anticipatedWorkHours))
        {
            return new List<Error>
            {
                new(ErrorCodes.InvalidAction, "Du har registrert en utbetaling som vil bli påvirket av denne timeføringen. Slett utbetalingen eller kontakt en admin for å få endret timene dine.")
            };
        }

        if (await FutureFlexWouldCauseNegativeBalance(timeEntriesOnDate.Values, currentUser.Id, anticipatedWorkHours, timeEntry))
        {
            return new List<Error> { new(ErrorCodes.InvalidAction, "Fremtidig avspasering vil resultere i negativ balanse.") };
        }

        timeEntriesOnDate[timeEntry.TaskId] = new TimeEntryResponseDto
        {
            Date = timeEntryDate,
            Value = timeEntry.Value,
            TaskId = timeEntry.TaskId
        };

        if (timeEntriesOnDate.Values.Sum(te => te.Value) > anticipatedWorkHours &&
            timeEntriesOnDate.Values.Any(te => te.TaskId == _flexTask && te.Value > 0))
        {
            return new List<Error>
            {
                new(ErrorCodes.InvalidAction, $"Du kan ikke registrere mer enn {anticipatedWorkHours:0.00} timer når du avspaserer.")
            };
        }

        if (timeEntry.TaskId == _flexTask)
        {
            if (latestPayoutDate != null && timeEntry.Date.Date <= latestPayoutDate)
            {
                return new List<Error>
                {
                    new(ErrorCodes.InvalidAction, "Du har registrert en utbetaling som vil bli påvirket av denne timeføringen. Slett utbetalingen eller kontakt en admin for å få endret timene dine.")
                };
            }
        }

        var taskGivesOvertime = await _taskUtils.TaskGivesOvertime(timeEntry.TaskId);
        if (timeEntry.Value > anticipatedWorkHours && !taskGivesOvertime)
        {
            return new List<Error>
            {
                new(ErrorCodes.InvalidAction, $"Du kan ikke registrere mer enn {anticipatedWorkHours:0.00} timer på den oppgaven.")
            };
        }

        if (!taskGivesOvertime &&
            timeEntry.Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return new List<Error> { new(ErrorCodes.InvalidAction, "Du kan ikke registrere den oppgaven på en helgedag.") };
        }

        return new List<Error>();
    }

    private async Task<bool> FutureFlexWouldCauseNegativeBalance(
        IEnumerable<TimeEntryResponseDto> timeEntriesOnDate,
        int currentUserId, decimal anticipatedWorkHours, CreateTimeEntryDto createTimeEntryDto)
    {
        var date = createTimeEntryDto.Date.Date;
        var totalAvailableOvertime = await GetAvailableOvertimeHoursAtDate(new DateTime(9999, 01, 01));

        if (timeEntriesOnDate.Any(te => te.TaskId == _flexTask) && createTimeEntryDto.TaskId == _flexTask)
        {
            var diffInFlex = createTimeEntryDto.Value -
                             timeEntriesOnDate.Where(f => f.TaskId == _flexTask).Sum(f => f.Value);
            return diffInFlex > 0 && totalAvailableOvertime.AvailableHoursBeforeCompensation - diffInFlex < 0;
        }

        if (createTimeEntryDto.TaskId == _flexTask)
        {
            if (totalAvailableOvertime.AvailableHoursBeforeCompensation - createTimeEntryDto.Value < 0)
            {
                return true;
            }
        }

        var previousOvertimeOnDate = await _timeRegistrationStorage.GetEarnedOvertime(new OvertimeQueryFilter
        {
            UserId = currentUserId,
            FromDateInclusive = date,
            ToDateInclusive = date
        });

        if (previousOvertimeOnDate.Any())
        {
            var oldTotalOnDay = timeEntriesOnDate.Sum(t => t.Value);
            var previousOvertimeSumOnDay = previousOvertimeOnDate.Sum(ot => ot.Value);
            var oldValue = timeEntriesOnDate.FirstOrDefault(te => te.TaskId == createTimeEntryDto.TaskId);
            var diffOnTask = oldValue == null ? 0 : oldValue.Value - createTimeEntryDto.Value;

            var newTotalOnDay = oldTotalOnDay - diffOnTask;

            var futureFlex = (await _timeRegistrationStorage.GetFlexEntries(new TimeEntryQuerySearch
            {
                UserId = currentUserId,
                FromDateInclusive = date,
            })).ToList();

            if (newTotalOnDay < oldTotalOnDay && futureFlex.Any())
            {
                foreach (var flexEntry in futureFlex)
                {
                    var availableOtOnDay = await GetAvailableOvertimeHoursAtDate(flexEntry.Date);
                    var diffInOvertime = newTotalOnDay < anticipatedWorkHours
                        ? oldTotalOnDay - anticipatedWorkHours
                        : oldTotalOnDay - newTotalOnDay;
                    var newAvailableOvertimeAfterChange =
                        availableOtOnDay.AvailableHoursBeforeCompensation - diffInOvertime;

                    if (newAvailableOvertimeAfterChange < flexEntry.Hours)
                    {
                        return true;
                    }
                }
            }

            if (newTotalOnDay > anticipatedWorkHours && newTotalOnDay - anticipatedWorkHours < previousOvertimeSumOnDay)
            {
                var diffOtOnDay = previousOvertimeSumOnDay - (newTotalOnDay - anticipatedWorkHours);
                if (totalAvailableOvertime.AvailableHoursBeforeCompensation - diffOtOnDay < 0)
                {
                    return true;
                }
            }

            if (newTotalOnDay <= anticipatedWorkHours)
            {
                if (totalAvailableOvertime.AvailableHoursBeforeCompensation - previousOvertimeSumOnDay < 0)
                {
                    return true;
                }
            }
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
        DateTime date)
    {
        var entriesOnDay = (await _timeRegistrationStorage.GetTimeEntriesWithCompensationRate(
            new TimeEntryQuerySearch
            {
                UserId = userId,
                FromDateInclusive = date.Date,
                ToDateInclusive = date.Date
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

    public async Task<List<TimeEntry>> GetFlexTimeEntries()
    {
        var currentUser = await _userContext.GetCurrentUser();
        var flex = (await _timeRegistrationStorage.GetFlexEntries(new TimeEntryQuerySearch
        {
            UserId = currentUser.Id
        })).ToList();
        return flex;
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
        return await GetAvailableOvertimeHoursAtDate(toDateInclusive, currentUser);
    }

    public async Task<AvailableOvertimeDto> GetAvailableOvertimeHoursAtDate(DateTime toDateInclusive, User currentUser)
    {
        var earnedOvertime = await _timeRegistrationStorage.GetEarnedOvertime(new OvertimeQueryFilter
        {
            UserId = currentUser.Id,
            ToDateInclusive = toDateInclusive.Date
        });

        var overtimeEntries = new List<TimeEntry>();
        overtimeEntries.AddRange(earnedOvertime.Select(eo => new TimeEntry
        {
            Date = eo.Date,
            Hours = eo.Value,
            CompensationRate = eo.CompensationRate,
            Type = TimeEntryType.Overtime
        }));
        var overtimeEntriesOnly = new List<TimeEntry>(overtimeEntries);

        var compensatedPayouts = await CompensateForPayouts(overtimeEntries, toDateInclusive, currentUser);
        var compensatedFlexHours = await CompensateForFlexedHours(overtimeEntries, toDateInclusive, currentUser);

        var availableBeforeCompRate = overtimeEntries.Sum(e => e.Hours);
        var availableAfterCompRate = overtimeEntries.Sum(e => e.Hours * e.CompensationRate);

        return new AvailableOvertimeDto
        {
            UnCompensatedOvertime = overtimeEntriesOnly,
            CompensatedPayouts = compensatedPayouts,
            CompensatedFlexHours = compensatedFlexHours,
            AvailableHoursBeforeCompensation = availableBeforeCompRate,
            AvailableHoursAfterCompensation = availableAfterCompRate,
            Entries = overtimeEntries
        };
    }

    private async Task<List<TimeEntry>> CompensateForFlexedHours(List<TimeEntry> timeEntries, DateTime toDateInclusive, User currentUser = null)
    {
        if (currentUser is null)
        {
            currentUser = await _userContext.GetCurrentUser();
        }

        var flex = (await _timeRegistrationStorage.GetFlexEntries(new TimeEntryQuerySearch
        {
            UserId = currentUser.Id,
            FromDateInclusive = currentUser.StartDate.Date,
            ToDateInclusive = toDateInclusive.Date
        })).ToList();

        var compensatedFlexHours = flex.Select(x => new TimeEntry
        {
            Date = x.Date,
            CompensationRate = x.CompensationRate,
            Hours = -x.Hours,
            Type = TimeEntryType.Flex
        }).ToList();

        timeEntries.AddRange(compensatedFlexHours);

        return compensatedFlexHours;
    }

    private async Task<List<TimeEntry>> CompensateForPayouts(List<TimeEntry> overtimeEntries,
        DateTime toDateInclusive,
        User currentUser = null)
    {
        if (currentUser is null)
        {
            currentUser = await _userContext.GetCurrentUser();
        }

        var compensatedPayouts = new List<TimeEntry>();

        var registeredPayouts = await _payoutStorage.GetRegisteredPayouts(new PayoutQueryFilter
        {
            UserId = currentUser.Id,
            ToDateInclusive = toDateInclusive.Date
        });

        var payoutEntriesGroupedByDateAndRate =
            registeredPayouts.Entries.GroupBy(e => (e.Date, e.CompRate)).OrderBy(g => g.Key.Date);

        foreach (var payoutEntryGroup in payoutEntriesGroupedByDateAndRate)
        {
            var payoutEntry = new TimeEntry
            {
                Hours = -payoutEntryGroup.Sum(po => po.HoursBeforeCompRate),
                CompensationRate = payoutEntryGroup.Key.CompRate,
                Date = payoutEntryGroup.Key.Date,
                Type = TimeEntryType.Payout,
                Active = payoutEntryGroup.First().Active
            };

            overtimeEntries.Add(payoutEntry);
            compensatedPayouts.Add(payoutEntry);
        }

        return compensatedPayouts;
    }

    private async Task UpdateEarnedOvertime(List<TimeEntryWithCompRateDto> timeEntriesOnDay, int userId)
    {
        var timeEntryDate = timeEntriesOnDay.First().Date.Date;
        await _timeRegistrationStorage.DeleteOvertimeOnDate(timeEntryDate, userId);
        await StoreNewOvertime(timeEntriesOnDay);
    }

    private async Task StoreNewOvertime(List<TimeEntryWithCompRateDto> timeEntriesOnDay)
    {
        var currentUser = await _userContext.GetCurrentUser();

        var timeEntryDate = timeEntriesOnDay.First().Date.Date;
        var allRedDays = new RedDays(timeEntryDate.Year).Dates;

        var usersEmploymentRateResult = await _userService.GetCurrentEmploymentRateForUser(currentUser.Id, timeEntryDate);
        if (!usersEmploymentRateResult.IsSuccess)
        {
            return;
        }

        var anticipatedWorkHours =
            IsWeekend(timeEntryDate) || allRedDays.Contains(timeEntryDate)
                ? 0M
                : HoursInWorkday * usersEmploymentRateResult.Value;

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
        new Result();
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