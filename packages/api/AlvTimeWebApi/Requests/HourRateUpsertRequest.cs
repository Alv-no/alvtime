using System;
using System.ComponentModel.DataAnnotations;

namespace AlvTimeWebApi.Requests;

public class HourRateUpsertRequest
{
    [Required]
    public DateTime FromDate { get; set; }
    [Required]
    public decimal Rate { get; set; }
}