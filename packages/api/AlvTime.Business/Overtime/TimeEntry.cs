using System;

namespace AlvTime.Business.Overtime;

public enum TimeEntryType
{
    Overtime = 0,
    Payout = 1,
    Flex = 2
}

public class TimeEntry
{
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
    public decimal CompensationRate { get; set; }
    public TimeEntryType Type { get; set; } = TimeEntryType.Overtime;
    public bool? Active { get; set; }
}