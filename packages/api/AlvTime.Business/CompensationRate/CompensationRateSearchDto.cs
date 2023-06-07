using System;

namespace AlvTime.Business.CompensationRate;

public class CompensationRateSearchResultDto
{
    public int Id { get; set; }
    public DateTime FromDate { get; set; }
    public decimal Value { get; set; }
    public int TaskId { get; set; }
}