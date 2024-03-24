using System;
using System.ComponentModel.DataAnnotations;

namespace AlvTimeWebApi.Requests;

public class EmploymentRateUpsertRequest
{
    [Required]
    public decimal RatePercentage { get; set; }
    [Required]
    public DateTime FromDateInclusive { get; set; }
    [Required]
    public DateTime ToDateInclusive { get; set; }
}