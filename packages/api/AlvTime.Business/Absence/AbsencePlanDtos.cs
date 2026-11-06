using System;
using System.Collections.Generic;

namespace AlvTime.Business.Absence;

public enum AbsencePlanDayStatus
{
    Register,
    Weekend,
    RedDay,
    AlreadyRegistered,
    HasOtherHours,
    Blocked
}

public class AbsencePlanRequest
{
    public int TaskId { get; set; }
    public DateTime FromDateInclusive { get; set; }
    public DateTime ToDateInclusive { get; set; }
    public decimal Percentage { get; set; } = 100;
    public bool FillPartialDays { get; set; }
    public string? Comment { get; set; }
}

public class AbsencePlanTaskDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsPaidHoliday { get; set; }
}

public class AbsencePlanDayDto
{
    public DateTime Date { get; set; }
    public AbsencePlanDayStatus Status { get; set; }
    public decimal Hours { get; set; }
    public decimal ExistingOtherHours { get; set; }
    public string? Reason { get; set; }
}

public class AbsencePlanDto
{
    public List<AbsencePlanDayDto> Days { get; set; } = new();
    public int TotalDays { get; set; }
    public decimal TotalHours { get; set; }
}
