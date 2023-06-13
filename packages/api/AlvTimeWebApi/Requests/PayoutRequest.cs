using System;
using AlvTime.Business.Validators;

namespace AlvTimeWebApi.Requests;

public class PayoutRequest
{
    public DateTime Date { get; set; }
        
    public decimal Hours { get; set; }
}