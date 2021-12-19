using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.Overtime;
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
        private readonly int _flexTask;
        private readonly int _paidHolidayTask;
        private const decimal HoursInWorkday = 7.5M;

        public TimeRegistrationService(
            IOptionsMonitor<TimeEntryOptions> timeEntryOptions,
            IUserContext userContext,
            TaskUtils taskUtils,
            ITimeRegistrationStorage timeRegistrationStorage)
        {
            _userContext = userContext;
            _taskUtils = taskUtils;
            _timeRegistrationStorage = timeRegistrationStorage;
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
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        var createdTimeEntry = _timeRegistrationStorage.CreateTimeEntry(timeEntry, userId);
                        response.Add(createdTimeEntry);
                        var entriesOnDay = GetEntriesWithCompRatesForUserOnDay(userId, timeEntry);
                        UpdateEarnedOvertime(entriesOnDay);
                        transaction.Complete();
                    }
                }
                else
                {
                    var updatedTimeEntry = _timeRegistrationStorage.UpdateTimeEntry(timeEntry, userId);
                    response.Add(updatedTimeEntry);
                    var entriesOnDay = GetEntriesWithCompRatesForUserOnDay(userId, timeEntry);
                    UpdateEarnedOvertime(entriesOnDay);
                }
            }

            return response;
        }

        private void ValidateTimeEntry(CreateTimeEntryDto timeEntry)
        {
            if (timeEntry.TaskId == _flexTask)
            {
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
        public AvailableHoursDto GetAvailableOvertimeHours()
        {
            var currentUser = _userContext.GetCurrentUser();

            var availableHours =
                _timeRegistrationStorage.GetAvailableHours(currentUser.Id, currentUser.StartDate, DateTime.Now);

            return new AvailableHoursDto
            {
                AvailableHoursBeforeCompensation = 0,
                AvailableHoursAfterCompensation = 0,
                Entries = null
            };
        }

        //TODO: IMPLEMENT
        public AvailableHoursDto GetAvailableOvertimeHoursAtDate(DateTime toDateInclusive)
        {
            var currentUser = _userContext.GetCurrentUser();

            var availableHours =
                _timeRegistrationStorage.GetAvailableHours(currentUser.Id, currentUser.StartDate, toDateInclusive);

            return new AvailableHoursDto
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