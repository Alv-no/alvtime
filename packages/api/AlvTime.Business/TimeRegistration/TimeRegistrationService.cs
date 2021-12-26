using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using AlvTime.Business.CompensationRate;
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
        private readonly ICompensationRateStorage _compensationRateStorage;
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
            ICompensationRateStorage compensationRateStorage)
        {
            _userContext = userContext;
            _taskUtils = taskUtils;
            _timeRegistrationStorage = timeRegistrationStorage;
            _dbContextScope = dbContextScope;
            _payoutStorage = payoutStorage;
            _compensationRateStorage = compensationRateStorage;
            _flexTask = timeEntryOptions.CurrentValue.FlexTask;
            _paidHolidayTask = timeEntryOptions.CurrentValue.PaidHolidayTask;
        }

        public IEnumerable<TimeEntriesResponseDto> UpsertTimeEntry(IEnumerable<CreateTimeEntryDto> timeEntries)
        {
            var currentUser = _userContext.GetCurrentUser();
            var userId = currentUser.Id;

            List<TimeEntriesResponseDto> response = new List<TimeEntriesResponseDto>();

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
            });
            
            var latestPayoutDate = _payoutStorage.GetRegisteredPayouts(_userContext.GetCurrentUser().Id).Entries
                .OrderBy(po => po.Date).LastOrDefault()?.Date.Date;
            
            var allRedDays = new RedDays(timeEntry.Date.Year).Dates;
            var anticipatedWorkHours =
                IsWeekend(timeEntry.Date.Date) || allRedDays.Contains(timeEntry.Date.Date) ? 0M : HoursInWorkday;

            if (PayoutWouldBeAffectedByRegistration(timeEntry, latestPayoutDate, timeEntriesOnDate, anticipatedWorkHours))
            {
                throw new Exception("You have a registered payout that would be affected by this action. Please contact an admin to register your hours.");
            }
            
            if (timeEntry.TaskId == _flexTask)
            {
                if (latestPayoutDate != null && timeEntry.Date.Date < latestPayoutDate)
                {
                    throw new Exception("You have a registered payout that would be affected by this action. Please contact an admin to register your hours.");
                }
                
                var availableHours = GetAvailableOvertimeHoursAtDate(timeEntry.Date.Date);
                var availableForFlex = availableHours.AvailableHoursBeforeCompensation;

                if (timeEntry.Value > availableForFlex)
                {
                    throw new Exception("Not enough available hours to flex");
                }
            }

            if (timeEntry.Value > HoursInWorkday && !_taskUtils.TaskGivesOvertime(timeEntry.TaskId))
            {
                throw new Exception("You cannot register more than 7.5 hours on that task");
            }

            if (timeEntry.TaskId == _paidHolidayTask &&
                (timeEntry.Date.DayOfWeek == DayOfWeek.Saturday || timeEntry.Date.DayOfWeek == DayOfWeek.Sunday))
            {
                throw new Exception("You cannot register vacation on a weekend");
            }
        }

        private static bool PayoutWouldBeAffectedByRegistration(CreateTimeEntryDto timeEntry, DateTime? latestPayoutDate, IEnumerable<TimeEntriesResponseDto> timeEntriesOnDate, decimal anticipatedWorkHours)
        {
            return latestPayoutDate != null && timeEntry.Date.Date < latestPayoutDate && timeEntriesOnDate.Sum(te => te.Value) + timeEntry.Value > anticipatedWorkHours;
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

        private TimeEntriesResponseDto GetTimeEntry(TimeEntryQuerySearch criterias)
        {
            return _timeRegistrationStorage.GetTimeEntry(criterias);
        }

        public List<EarnedOvertimeDto> GetEarnedOvertime(OvertimeQueryFilter criterias)
        {
            var currentUser = _userContext.GetCurrentUser();
            criterias.UserId = currentUser.Id;
            return _timeRegistrationStorage.GetEarnedOvertime(criterias);
        }

        //TODO: IMPLEMENT
        public AvailableOvertimeDto GetAvailableOvertimeHoursNow()
        {
            return GetAvailableOvertimeHoursAtDate(DateTime.Now);
        }

        public AvailableOvertimeDto GetAvailableOvertimeHoursAtDate(DateTime toDateInclusive)
        {
            var currentUser = _userContext.GetCurrentUser();

            var flexedHours = _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                UserId = currentUser.Id,
                TaskId = _flexTask,
                FromDateInclusive = currentUser.StartDate.Date,
                ToDateInclusive = toDateInclusive.Date
            }).Sum(flex => flex.Value);

            var flexHourCompRate = _compensationRateStorage.GetCompensationRates(new CompensationRateQuerySearch
            {
                TaskId = _flexTask
            }).OrderBy(cr => cr.FromDate).Last();

            var registeredPayouts = _payoutStorage.GetRegisteredPayouts(currentUser.Id);

            var earnedOvertime = _timeRegistrationStorage.GetEarnedOvertime(new OvertimeQueryFilter
            {
                UserId = currentUser.Id,
                StartDate = currentUser.StartDate.Date,
                EndDate = toDateInclusive.Date
            });
            
            var groupedOvertime = earnedOvertime.GroupBy(eo => eo.CompensationRate).Select(eo => new
            {
                CompensationRate = eo.Key,
                Hours = eo.Sum(entry => entry.Value)                
            }).OrderBy(e => e.CompensationRate);
            
            // Bruk CompensateForPayouts fra Flex
            // Group alt på dato og start på starten

            return new AvailableOvertimeDto
            {
                AvailableHoursBeforeCompensation = 0,
                AvailableHoursAfterCompensation = 0,
                Entries = null
            };
        }

        public List<OvertimeEntry> UpdateEarnedOvertime(List<TimeEntryWithCompRateDto> timeEntriesOnDay)
        {
            var currentUser = _userContext.GetCurrentUser();
            var timeEntryDate = timeEntriesOnDay.First().Date.Date;
            _timeRegistrationStorage.DeleteOvertimeOnDate(timeEntryDate, currentUser.Id);

            return StoreNewOvertime(timeEntriesOnDay);
        }

        public List<OvertimeEntry> StoreNewOvertime(List<TimeEntryWithCompRateDto> timeEntriesOnDay)
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
                if (anticipatedWorkHours == 0 && !_taskUtils.TaskGivesOvertime(timeEntry.TaskId))
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

                if (normalWorkHoursLeft <= 0)
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

            return overtimeEntries;
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}