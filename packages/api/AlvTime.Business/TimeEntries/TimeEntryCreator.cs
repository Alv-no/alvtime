using System.Collections.Generic;

namespace AlvTime.Business.TimeEntries
{
    public class TimeEntryCreator
    {
        private readonly ITimeEntryStorage _storage;

        public TimeEntryCreator(ITimeEntryStorage storage)
        {
            _storage = storage;
        }

        public IEnumerable<TimeEntriesResponseDto> UpsertTimeEntry(IEnumerable<CreateTimeEntryDto> timeEntries, int userId)
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

                if (GetTimeEntry(criterias) == null)
                {
                    response.Add(_storage.CreateTimeEntry(timeEntry, userId));
                }
                else
                {
                    response.Add(_storage.UpdateTimeEntry(timeEntry, userId));
                }
            }

            return response;
        }

        public TimeEntriesResponseDto GetTimeEntry(TimeEntryQuerySearch criterias)
        {
            return _storage.GetTimeEntry(criterias);
        } 
    }
}
