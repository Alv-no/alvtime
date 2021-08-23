using System;

namespace AlvTime.Business.EconomyData
{
    public class RegisterOvertimePayoutDto
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPayout { get; set; }
        public int PaidOvertimeId { get; set; }
    }
}
