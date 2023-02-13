using System;

namespace AlvTime.Business.Users;

public class EmploymentRateChangeRequestDto
{
    public int UserId { get; set; }
    public int RateId { get; set; }
    public decimal Rate { get; set; }
    public DateTime FromDateInclusive { get; set; }
    public DateTime ToDateInclusive { get; set; }
}