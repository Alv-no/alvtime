using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task<List<EarnedOvertimeDto>> GetEarnedOvertime(OvertimeQueryFilter criteria);
        Task StoreOvertime(List<OvertimeEntry> overtimeEntries, int userId);
        Task DeleteOvertimeOnDate(DateTime date, int userId);
        Task<List<TimeEntryWithCustomerDto>> GetTimeEntriesWithCustomer(int userId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<TimeEntry>> GetFlexEntries(TimeEntryQuerySearch criteria);
        Task RegisterFlex(TimeEntry timeEntry, int userId);
        Task DeleteFlexOnDate(DateTime dateTime, int userId);
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
        public DateTime? FromDateInclusive { get; set; }
        public DateTime? ToDateInclusive { get; set; }
        public decimal? CompensationRate { get; set; }
    }
}
