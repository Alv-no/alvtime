using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeEntries;

namespace AlvTime.Business.TimeRegistration
{
    public interface ITimeRegistrationStorage
    {
        Task<IEnumerable<TimeEntryResponseDto>> GetTimeEntries(TimeEntryQuerySearch criteria);
        Task<IEnumerable<TimeEntryWithCompRateDto>> GetTimeEntriesWithCompensationRate(TimeEntryQuerySearch criteria);
        Task<TimeEntryResponseDto> GetTimeEntry(TimeEntryQuerySearch criteria);
        Task<TimeEntryResponseDto> CreateTimeEntry(CreateTimeEntryDto timeEntry, int userId);
        Task<TimeEntryResponseDto> UpdateTimeEntry(CreateTimeEntryDto timeEntry, int userId);
        Task<IEnumerable<DateEntry>> GetDateEntries(TimeEntryQuerySearch criteria);
        Task<List<EarnedOvertimeDto>> GetEarnedOvertime(OvertimeQueryFilter criteria);
        Task StoreOvertime(List<OvertimeEntry> overtimeEntries, int userId);
        Task DeleteOvertimeOnDate(DateTime date, int userId);
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
