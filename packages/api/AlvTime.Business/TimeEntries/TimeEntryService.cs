using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace AlvTime.Business.TimeEntries
{
    public class TimeEntryCreator
    {
        private readonly ITimeEntryStorage _timeEntryStorage;
        private readonly IFlexhourStorage _flexHourStorage;
        private readonly IOptionsMonitor<TimeEntryOptions> _timeEntryOptions;
        private readonly int _flexTask;

        public TimeEntryCreator(ITimeEntryStorage timeEntryStorage, IFlexhourStorage flexHourStorage, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
        {
            _timeEntryStorage = timeEntryStorage;
            _flexHourStorage = flexHourStorage;
            _timeEntryOptions = timeEntryOptions;
            _flexTask = _timeEntryOptions.CurrentValue.FlexTask;
        }

        public IEnumerable<TimeEntriesResponseDto> UpsertTimeEntry(IEnumerable<CreateTimeEntryDto> timeEntries, int userId, DateTime startDate)
        {
            List<TimeEntriesResponseDto> response = new List<TimeEntriesResponseDto>();

            foreach (var timeEntry in timeEntries)
            {
                var criterias = new TimeEntryQuerySearch
                {
                    UserId = userId,
                    FromDateInclusive = timeEntry.Date,
                    ToDateInclusive = timeEntry.Date,
                    TaskId = timeEntry.TaskId
                };

                if (timeEntry.TaskId == _flexTask)
                {
                    var availableHours = _flexHourStorage.GetAvailableHours(userId, startDate, timeEntry.Date);

                    if (timeEntry.Value > availableHours.AvailableHoursBeforeCompensation)
                    {
                        throw new Exception("Not enough available hours to flex");
                    }
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
