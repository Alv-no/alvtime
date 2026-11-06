using System;

namespace AlvTimeWebApi.Requests;

public class AbsencePlanRequest
{
    public int TaskId { get; set; }
    public DateTime FromDateInclusive { get; set; }
    public DateTime ToDateInclusive { get; set; }
    public decimal Percentage { get; set; } = 100;
    public bool FillPartialDays { get; set; }
    public string? Comment { get; set; }
}
