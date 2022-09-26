using System;
using System.ComponentModel.DataAnnotations;

namespace AlvTime.Business.Users;

public class EmploymentRateChangeRequest
{
    [Required]
    public int RateId { get; set; }
    [Required]
    public decimal Rate { get; set; }
    [Required]
    public DateTime FromDateInclusive { get; set; }
    [Required]
    public DateTime ToDateInclusive { get; set; }
}