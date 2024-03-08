﻿using System.Collections.Generic;

namespace AlvTimeWebApi.Responses;

public class UserResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public int? EmployeeId { get; set; }
    public IEnumerable<UserEmploymentRateResponse> EmploymentRates { get; set; }
}

public class UserEmploymentRateResponse
{
    public int Id { get; set; }
    public decimal RatePercentage { get; set; }
    public string FromDateInclusive { get; set; }
    public string ToDateInclusive { get; set; }
}