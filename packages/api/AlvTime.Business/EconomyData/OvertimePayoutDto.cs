using System;

namespace AlvTime.Business.EconomyData
{
    public record OvertimePayoutDto(int Id, int UserId, DateTime Date, decimal TotalPayout, int PaidOvertimeId)
    {
    }
}
