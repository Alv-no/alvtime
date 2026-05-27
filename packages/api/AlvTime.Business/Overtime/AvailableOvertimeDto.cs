using System.Collections.Generic;

namespace AlvTime.Business.Overtime;

public class AvailableOvertimeDto
{
    public decimal AvailableHoursBeforeCompensation { get; set; }
    public decimal AvailableHoursAfterCompensation { get; set; }
    public decimal HoursUntilBankingStarts { get; set; }
    public List<TimeEntry> Entries { get; set; }
    public List<TimeEntry> CompensatedPayouts { get; internal set; }
    public List<TimeEntry> CompensatedFlexHours { get; internal set; }
    public List<TimeEntry> CompensatedFiscalYearDebt { get; internal set; }
    public List<TimeEntry> UnCompensatedOvertime { get; internal set; }
}