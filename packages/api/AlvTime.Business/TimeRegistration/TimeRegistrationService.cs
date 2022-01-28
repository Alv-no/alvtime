using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.Overtime;
using AlvTime.Business.Payouts;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.Utils;
using Microsoft.Extensions.Options;

namespace AlvTime.Business.TimeRegistration
{
    public class TimeRegistrationService
    {
        private readonly IUserContext _userContext;
        private readonly TaskUtils _taskUtils;
        private readonly ITimeRegistrationStorage _timeRegistrationStorage;
        private readonly IDbContextScope _dbContextScope;
        private readonly IPayoutStorage _payoutStorage;
        private readonly int _flexTask;
        private readonly int _paidHolidayTask;
        private const decimal HoursInWorkday = 7.5M;

        public TimeRegistrationService(
            IOptionsMonitor<TimeEntryOptions> timeEntryOptions,
            IUserContext userContext,
            TaskUtils taskUtils,
            ITimeRegistrationStorage timeRegistrationStorage,
            IDbContextScope dbContextScope,
            IPayoutStorage payoutStorage)
        {
            _userContext = userContext;
            _taskUtils = taskUtils;
            _timeRegistrationStorage = timeRegistrationStorage;
            _dbContextScope = dbContextScope;
            _payoutStorage = payoutStorage;
            _flexTask = timeEntryOptions.CurrentValue.FlexTask;
            _paidHolidayTask = timeEntryOptions.CurrentValue.PaidHolidayTask;
        }

        public IEnumerable<TimeEntryResponseDto> UpsertTimeEntry(IEnumerable<CreateTimeEntryDto> timeEntries)
        {
            var currentUser = _userContext.GetCurrentUser();
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

                ValidateTimeEntry(timeEntry);

                if (GetTimeEntry(criterias) == null)
                {
                    _dbContextScope.AsAtomic(() =>
                    {
                        var createdTimeEntry = _timeRegistrationStorage.CreateTimeEntry(timeEntry, userId);
                        response.Add(createdTimeEntry);
                        var entriesOnDay = GetEntriesWithCompRatesForUserOnDay(userId, timeEntry);
                        UpdateEarnedOvertime(entriesOnDay);
                    });
                }
                else
                {
                    _dbContextScope.AsAtomic(() =>
                    {
                        var updatedTimeEntry = _timeRegistrationStorage.UpdateTimeEntry(timeEntry, userId);
                        response.Add(updatedTimeEntry);
                        var entriesOnDay = GetEntriesWithCompRatesForUserOnDay(userId, timeEntry);
                        UpdateEarnedOvertime(entriesOnDay);
                    });
                }
            }

            return response;
        }

        private void ValidateTimeEntry(CreateTimeEntryDto timeEntry)
        {
            var timeEntriesOnDate = _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = timeEntry.Date.Date,
                ToDateInclusive = timeEntry.Date.Date,
                UserId = _userContext.GetCurrentUser().Id
            }).ToDictionary(entry => entry.TaskId, entry => entry);
            
            var latestPayoutDate = _payoutStorage
                .GetRegisteredPayouts(new PayoutQueryFilter {UserId = _userContext.GetCurrentUser().Id}).Entries
                .OrderBy(po => po.Date).LastOrDefault()?.Date.Date;
            
            var allRedDays = new RedDays(timeEntry.Date.Year).Dates;
            var anticipatedWorkHours =
                IsWeekend(timeEntry.Date.Date) || allRedDays.Contains(timeEntry.Date.Date) ? 0M : HoursInWorkday;
            
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
                throw new Exception($"Du kan ikke registrere mer enn {anticipatedWorkHours} timer når du avspaserer.");
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

                var availableHours = GetAvailableOvertimeHoursAtDate(timeEntry.Date.Date);
                var availableForFlex = availableHours.AvailableHoursBeforeCompensation;

                if (timeEntry.Value > availableForFlex)
                {
                    throw new Exception("Ikke nok tilgjengelige timer til å avspasere.");
                }
            }

            if (timeEntry.Value > HoursInWorkday && !_taskUtils.TaskGivesOvertime(timeEntry.TaskId))
            {
                throw new Exception("Du kan ikke registrere mer enn 7.5 timer på den oppgaven.");
            }

            if (!_taskUtils.TaskGivesOvertime(timeEntry.TaskId) &&
                (timeEntry.Date.DayOfWeek == DayOfWeek.Saturday || timeEntry.Date.DayOfWeek == DayOfWeek.Sunday))
            {
                throw new Exception("Du kan ikke registrere den oppgaven på en helg.");
            }
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

        private List<TimeEntryWithCompRateDto> GetEntriesWithCompRatesForUserOnDay(int userId,
            CreateTimeEntryDto timeEntry)
        {
            var entriesOnDay = _timeRegistrationStorage.GetTimeEntriesWithCompensationRate(
                new TimeEntryQuerySearch
                {
                    UserId = userId,
                    FromDateInclusive = timeEntry.Date.Date,
                    ToDateInclusive = timeEntry.Date.Date
                }).ToList();
            return entriesOnDay;
        }

        private TimeEntryResponseDto GetTimeEntry(TimeEntryQuerySearch criterias)
        {
            return _timeRegistrationStorage.GetTimeEntry(criterias);
        }

        public List<TimeEntryResponseDto> GetTimeEntries(TimeEntryQuerySearch criterias)
        {
            var currentUser = _userContext.GetCurrentUser();
            criterias.UserId = currentUser.Id;
            return _timeRegistrationStorage.GetTimeEntries(criterias).ToList();
        }

        public List<EarnedOvertimeDto> GetEarnedOvertime(OvertimeQueryFilter criterias)
        {
            var currentUser = _userContext.GetCurrentUser();
            criterias.UserId = currentUser.Id;
            return _timeRegistrationStorage.GetEarnedOvertime(criterias);
        }

        public AvailableOvertimeDto GetAvailableOvertimeHoursNow()
        {
            return GetAvailableOvertimeHoursAtDate(DateTime.Now);
        }

        public AvailableOvertimeDto GetAvailableOvertimeHoursAtDate(DateTime toDateInclusive)
        {
            var currentUser = _userContext.GetCurrentUser();

            var earnedOvertime = _timeRegistrationStorage.GetEarnedOvertime(new OvertimeQueryFilter
            {
                UserId = currentUser.Id,
                EndDate = toDateInclusive.Date
            });

            var timeEntries = new List<TimeEntry>();
            timeEntries.AddRange(earnedOvertime.Select(eo => new TimeEntry
            {
                Date = eo.Date,
                Hours = eo.Value,
                CompensationRate = eo.CompensationRate
            }));

            CompensateForFlexedHours(timeEntries, toDateInclusive);
            CompensateForPayouts(timeEntries, toDateInclusive);

            var availableBeforeCompRate = timeEntries.Sum(e => e.Hours);
            var availableAfterCompRate = timeEntries.Sum(e => e.Hours * e.CompensationRate);

            return new AvailableOvertimeDto
            {
                AvailableHoursBeforeCompensation = availableBeforeCompRate,
                AvailableHoursAfterCompensation = availableAfterCompRate,
                Entries = timeEntries
            };
        }

        private void CompensateForFlexedHours(List<TimeEntry> timeEntries, DateTime toDateInclusive)
        {
            var currentUser = _userContext.GetCurrentUser();

            var flexedTimeEntries = _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                UserId = currentUser.Id,
                TaskId = _flexTask,
                FromDateInclusive = currentUser.StartDate.Date,
                ToDateInclusive = toDateInclusive.Date
            }).ToList();

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

        private void CompensateForPayouts(List<TimeEntry> timeEntries, DateTime toDateInclusive)
        {
            var currentUser = _userContext.GetCurrentUser();

            var registeredPayouts = _payoutStorage.GetRegisteredPayouts(new PayoutQueryFilter
            {
                UserId = currentUser.Id,
                ToDateInclusive = toDateInclusive.Date
            });

            var payoutEntriesGroupedByDate = registeredPayouts.Entries.GroupBy(e => e.Date).OrderBy(g => g.Key);

            foreach (var payoutEntryGroup in payoutEntriesGroupedByDate)
            {
                var payoutDate = payoutEntryGroup.Key;
                var registeredPayoutsTotal = payoutEntryGroup.Sum(e => e.HoursBeforeCompRate);

                var relevantTimeEntriesOrdered = timeEntries.Where(e => e.Date <= payoutDate).GroupBy(
                        hours => hours.CompensationRate,
                        hours => hours,
                        (cr, hours) => new
                        {
                            CompensationRate = cr,
                            Hours = hours.Sum(h => h.Hours)
                        })
                    .OrderBy(h => h.CompensationRate);

                foreach (var entry in relevantTimeEntriesOrdered)
                {
                    if (registeredPayoutsTotal <= 0)
                    {
                        break;
                    }

                    TimeEntry overtimeEntry = new TimeEntry
                    {
                        Hours = -Math.Min(registeredPayoutsTotal, entry.Hours),
                        CompensationRate = entry.CompensationRate,
                        Date = payoutDate
                    };

                    timeEntries.Add(overtimeEntry);
                    registeredPayoutsTotal += overtimeEntry.Hours;
                }
            }
        }

        public void UpdateEarnedOvertime(List<TimeEntryWithCompRateDto> timeEntriesOnDay)
        {
            var currentUser = _userContext.GetCurrentUser();
            var timeEntryDate = timeEntriesOnDay.First().Date.Date;
            _timeRegistrationStorage.DeleteOvertimeOnDate(timeEntryDate, currentUser.Id);
            StoreNewOvertime(timeEntriesOnDay);
        }

        public void StoreNewOvertime(List<TimeEntryWithCompRateDto> timeEntriesOnDay)
        {
            var currentUser = _userContext.GetCurrentUser();

            var timeEntryDate = timeEntriesOnDay.First().Date.Date;
            var allRedDays = new RedDays(timeEntryDate.Year).Dates;

            var anticipatedWorkHours =
                IsWeekend(timeEntryDate) || allRedDays.Contains(timeEntryDate) ? 0M : HoursInWorkday;

            var normalWorkHoursLeft = anticipatedWorkHours;
            var overtimeEntries = new List<OvertimeEntry>();
            foreach (var timeEntry in timeEntriesOnDay.OrderByDescending(entry => entry.CompensationRate))
            {
                if (anticipatedWorkHours == 0 &&
                    !_taskUtils.TaskGivesOvertime(timeEntry.TaskId)) //Guard against absence overtime on red day
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

            _timeRegistrationStorage.StoreOvertime(overtimeEntries, currentUser.Id);
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}