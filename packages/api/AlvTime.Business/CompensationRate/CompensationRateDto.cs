using System;

namespace AlvTime.Business.CompensationRate;

public class CompensationRateDto
{
    public DateTime FromDate { get; set; }
    public decimal Value { get; set; }
    public int TaskId { get; set; }
}