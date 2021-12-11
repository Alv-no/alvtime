using System;
using System.Collections.Generic;

namespace AlvTime.Business.TimeEntries
{
    public interface ITimeEntryStorage
    {
        IEnumerable<TimeEntriesResponseDto> GetTimeEntries(TimeEntryQuerySearch criterias);
        IEnumerable<TimeEntriesWithCompRateResponseDto> GetTimeEntriesWithCompensationRate(TimeEntryQuerySearch criterias);
        TimeEntriesResponseDto GetTimeEntry(TimeEntryQuerySearch criterias);
        TimeEntriesResponseDto CreateTimeEntry(CreateTimeEntryDto timeEntry, int userId);
        TimeEntriesResponseDto UpdateTimeEntry(CreateTimeEntryDto timeEntry, int userId);
        IEnumerable<DateEntry> GetDateEntries(TimeEntryQuerySearch criterias);
    }

    public class TimeEntryQuerySearch
    {
        public int? UserId { get; set; }
        public int? Id { get; set; }
        public DateTime? FromDateInclusive { get; set; }
        public DateTime? ToDateInclusive { get; set; }
        public decimal? Value { get; set; }
        public int? TaskId { get; set; }
    }
}
