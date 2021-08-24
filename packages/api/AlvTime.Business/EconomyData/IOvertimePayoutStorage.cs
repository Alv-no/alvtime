namespace AlvTime.Business.EconomyData
{
    public interface IOvertimePayoutStorage
    {
        OvertimePayoutResponsDto DeleteOvertimePayout(int userId, int paidOvertimeId);
        OvertimePayoutResponsDto SaveOvertimePayout(RegisterOvertimePayoutDto overtimePayout);
    }
}
