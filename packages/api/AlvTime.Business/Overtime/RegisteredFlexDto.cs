using System;

namespace AlvTime.Business.Overtime;

public class RegisteredFlexDto
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public decimal CompensationRate { get; set; }
}