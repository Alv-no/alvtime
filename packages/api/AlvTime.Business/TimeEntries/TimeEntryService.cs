using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Utils;

namespace AlvTime.Business.TimeEntries
{
    public class TimeEntryService
    {
        private readonly ITimeEntryStorage _timeEntryStorage;
        private readonly IFlexhourStorage _flexHourStorage;
        private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;
        private readonly IUserContext _userContext;
        private readonly TaskUtils _taskUtils;
        private readonly int _flexTask;
        private const decimal HOURS_IN_WORKDAY = 7.5M;

        public TimeEntryService(
            ITimeEntryStorage timeEntryStorage, 
            IFlexhourStorage flexHourStorage, 
            IOptionsMonitor<TimeEntryOptions> timeEntryOptions, 
            IUserContext userContext, 
            TaskUtils taskUtils)
        {
            _timeEntryStorage = timeEntryStorage;
            _flexHourStorage = flexHourStorage;
            _timeEntryOptions = timeEntryOptions;
            _userContext = userContext;
            _taskUtils = taskUtils;
            _flexTask = _timeEntryOptions.CurrentValue.FlexTask;
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

                if (timeEntry.TaskId == _flexTask)
                {
                    var availableHours = _flexHourStorage.GetAvailableHours(userId, currentUser.StartDate, timeEntry.Date);

                    if (timeEntry.Value > availableHours.AvailableHoursBeforeCompensation)
                    {
                        throw new Exception("Not enough available hours to flex");
                    }
                }

                if (timeEntry.Value > HOURS_IN_WORKDAY  && !_taskUtils.TaskGivesOvertime(timeEntry.TaskId))
                {
                    throw new Exception("You cannot register more than 7.5 hours on that task");
                }

                if (GetTimeEntry(criterias) == null)
                {
                    response.Add(_timeEntryStorage.CreateTimeEntry(timeEntry, userId));
                }
                else
                {
                    response.Add(_timeEntryStorage.UpdateTimeEntry(timeEntry, userId));
                }
            }

            return response;
        }

        public TimeEntriesResponseDto GetTimeEntry(TimeEntryQuerySearch criterias)
        {
            return _timeEntryStorage.GetTimeEntry(criterias);
        }
    }
}
