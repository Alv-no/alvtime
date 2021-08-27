namespace AlvTime.Business.EconomyData
{
    public record OvertimePayoutRespons (int Id, int UserId, string Date, decimal TotalPayout, int PaidOvertimeId)
    {
    }
}