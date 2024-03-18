using System;
using System.Collections.Generic;

namespace AlvTime.Business.Users;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? EmployeeId { get; set; }
    public IEnumerable<UserEmploymentRateDto>? EmploymentRates { get; set; }
}

public class UserEmploymentRateDto
{
    public int Id { get; set; }
    public decimal Rate { get; set; }
    public DateTime FromDateInclusive { get; set; }
    public DateTime ToDateInclusive { get; set; }
}