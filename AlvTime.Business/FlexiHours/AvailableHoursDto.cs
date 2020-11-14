using System.Collections.Generic;

namespace AlvTime.Business.FlexiHours
{
    public class AvailableHoursDto
    {
        public decimal TotalHours { get; set; }
        public decimal TotalHoursIncludingCompensationRate { get; set; }
        public List<OvertimeEntry> Entries { get; set; }
    }
}
