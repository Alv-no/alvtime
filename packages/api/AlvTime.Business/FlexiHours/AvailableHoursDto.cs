using System.Collections.Generic;
using AlvTime.Business.Overtime;

namespace AlvTime.Business.FlexiHours
{
    public class AvailableHoursDto
    {
        public decimal AvailableHoursBeforeCompensation { get; set; }
        public decimal AvailableHoursAfterCompensation { get; set; }
        public List<OvertimeEntry> Entries { get; set; }
    }
}
