using System;

namespace AlvTime.Business.HourRates;

public class HourRateDto
{
    public int Id { get; set; }
    public DateTime FromDate { get; set; }
    public decimal Rate { get; set; }
}