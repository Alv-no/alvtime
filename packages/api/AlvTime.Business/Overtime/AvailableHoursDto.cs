using System.Collections.Generic;

namespace AlvTime.Business.Overtime;

public class AvailableHoursDto
{
    public decimal AvailableHoursBeforeCompensation { get; set; }
    public decimal AvailableHoursAfterCompensation { get; set; }
    public List<OvertimeEntry> Entries { get; set; }
}