using System;

namespace AlvTime.Business.EconomyData
{
    public record RegisterOvertimePayout
    {
        public int UserId { get; init; }
        public DateTime Date { get; init; }
        public decimal TotalPayout { get; init; }
        public int PaidOvertimeId { get; init; }
    }
}
