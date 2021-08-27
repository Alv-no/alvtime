using System;

namespace AlvTime.Business.EconomyData
{
    public record RegisterOvertimePayout (int UserId, DateTime Date, decimal TotalPayout, int PaidOvertimeId)
    {
    }
}