using System;

namespace AlvTime.Business.Overtime;

public class TimeEntry
{
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
    public decimal CompensationRate { get; set; }
}