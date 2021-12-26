using System;
using System.Collections.Generic;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeEntries;

namespace AlvTime.Business.TimeRegistration
{
    public interface ITimeRegistrationStorage
    {
        IEnumerable<TimeEntriesResponseDto> GetTimeEntries(TimeEntryQuerySearch criterias);
        IEnumerable<TimeEntryWithCompRateDto> GetTimeEntriesWithCompensationRate(TimeEntryQuerySearch criterias);
        TimeEntriesResponseDto GetTimeEntry(TimeEntryQuerySearch criterias);
        TimeEntriesResponseDto CreateTimeEntry(CreateTimeEntryDto timeEntry, int userId);
        TimeEntriesResponseDto UpdateTimeEntry(CreateTimeEntryDto timeEntry, int userId);
        IEnumerable<DateEntry> GetDateEntries(TimeEntryQuerySearch criterias);
        List<EarnedOvertimeDto> GetEarnedOvertime(OvertimeQueryFilter criterias);
        void StoreOvertime(List<OvertimeEntry> overtimeEntries, int userId);
        void DeleteOvertimeOnDate(DateTime date, int userId);
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
    
    public class OvertimeQueryFilter
    {
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? CompensationRate { get; set; }
    }
}
