using System;
using System.ComponentModel.DataAnnotations;

namespace AlvTimeWebApi.Requests;

public class EmploymentRateUpdateRequest
{
    [Required]
    public int Id { get; set; }
    [Required]
    public decimal RatePercentage { get; set; }
    [Required]
    public DateTime FromDateInclusive { get; set; }
    [Required]
    public DateTime ToDateInclusive { get; set; }
}