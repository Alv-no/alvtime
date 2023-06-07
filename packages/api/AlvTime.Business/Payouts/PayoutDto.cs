using System;

namespace AlvTime.Business.Payouts;

public class PayoutDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int UserId { get; set; }
    public decimal HoursBeforeCompensation { get; set; }
    public decimal HoursAfterCompensation { get; set; }
}