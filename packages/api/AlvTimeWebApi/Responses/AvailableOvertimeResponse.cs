using System.Collections.Generic;

namespace AlvTimeWebApi.Responses
{
    public class AvailableOvertimeResponse
    {
        public decimal AvailableHoursBeforeCompensation { get; set; }
        public decimal AvailableHoursAfterCompensation { get; set; }
        public List<TimeEntryResponse> Entries { get; set; }
    }
}