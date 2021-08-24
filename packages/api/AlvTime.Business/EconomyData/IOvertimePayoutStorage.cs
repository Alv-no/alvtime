namespace AlvTime.Business.EconomyData
{
    public interface IOvertimePayoutStorage
    {
        OvertimePayoutRespons DeleteOvertimePayout(int userId, int paidOvertimeId);
        OvertimePayoutRespons SaveOvertimePayout(RegisterOvertimePayout overtimePayout);
    }
}
