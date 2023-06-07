using System.Collections.Generic;
using AlvTime.Business.TimeRegistration.TimeEntries;

namespace AlvTime.Business.Absence;

public class VacationOverviewDto
{
    public decimal TotalHoursUsed { get; set; }
    public int TotalDaysUsed { get; set; }
    public IEnumerable<TimeEntryResponseDto> Entries { get; set; }
}