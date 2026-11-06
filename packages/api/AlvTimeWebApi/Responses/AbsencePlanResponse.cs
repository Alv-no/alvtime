using System.Collections.Generic;

namespace AlvTimeWebApi.Responses;

public class AbsencePlanResponse
{
    public List<AbsencePlanDayResponse> Days { get; set; } = new();
    public int TotalDays { get; set; }
    public decimal TotalHours { get; set; }
}

public class AbsencePlanDayResponse
{
    public string Date { get; set; }
    public string Status { get; set; }
    public decimal Hours { get; set; }
    public decimal ExistingOtherHours { get; set; }
    public string? Reason { get; set; }
}

public class AbsencePlanTaskResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsPaidHoliday { get; set; }
}
