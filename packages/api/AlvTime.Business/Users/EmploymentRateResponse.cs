using System;

namespace AlvTime.Business.Users;

public class EmploymentRateResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Rate { get; set; }
    public DateTime FromDateInclusive { get; set; }
    public DateTime ToDateInclusive { get; set; }
}