using System;

namespace AlvTimeWebApi.Requests;

public class PayoutRequest
{
    public DateTime Date { get; set; }
        
    public decimal Hours { get; set; }
}