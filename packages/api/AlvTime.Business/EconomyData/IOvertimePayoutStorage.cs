namespace AlvTime.Business.EconomyData
{
    public interface IOvertimePayoutStorage
    {
        OvertimePayoutDto DeleteOvertimePayout(int userId, int paidOvertimeId);
        OvertimePayoutDto SaveOvertimePayout(RegisterOvertimePayout overtimePayout);
    }
}
