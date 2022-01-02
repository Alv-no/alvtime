using System.Collections.Generic;

namespace AlvTime.Business.TimeEntries
{
    public class VacationOverviewDto
    {
        public decimal TotalHoursUsed { get; set; }
        public int TotalDaysUsed { get; set; }
        public IEnumerable<TimeEntryResponseDto> Entries { get; set; }
    }
}
