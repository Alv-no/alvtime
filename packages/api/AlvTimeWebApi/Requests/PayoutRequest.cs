using System;
using AlvTimeWebApi.Validators;

namespace AlvTimeWebApi.Requests;

public class PayoutRequest
{
    public DateTime Date { get; set; }
        
    [HalfHour]
    public decimal Hours { get; set; }
}