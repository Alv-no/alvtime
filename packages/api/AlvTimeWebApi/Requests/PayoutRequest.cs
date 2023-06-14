using System;
using AlvTimeWebApi.Validators;

namespace AlvTimeWebApi.Requests;

public class PayoutRequest
{
    public DateTime Date { get; set; }

    [QuarterHour]
    public decimal Hours { get; set; }
}