using System.Collections.Generic;

namespace AlvTime.Business.Overtime
{
    public class AvailableOvertimeDto
    {
        public decimal AvailableHoursBeforeCompensation { get; set; }
        public decimal AvailableHoursAfterCompensation { get; set; }
        public List<TimeEntry> Entries { get; set; }
    }
}
