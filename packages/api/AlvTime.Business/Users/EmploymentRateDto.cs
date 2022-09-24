using System;

namespace AlvTime.Business.Users;

public class EmploymentRateDto
{
    public int UserId { get; set; }
    public decimal Rate { get; set; }
    public DateTime FromDateInclusive { get; set; }
    public DateTime ToDateInclusive { get; set; }
}