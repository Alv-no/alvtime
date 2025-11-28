using System;
using System.ComponentModel.DataAnnotations;
using AlvTime.Business.Validators;

namespace AlvTime.Business.TimeRegistration;

public class CreateTimeEntryDto
{
    public DateTime Date { get; set; }

    [QuarterHour]
    [Range(0, int.MaxValue, ErrorMessage = "Kun positive tall er tillatt")]
    public decimal Value { get; set; }

    public int TaskId { get; set; }

    public string? Comment { get; set; }
}