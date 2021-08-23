namespace AlvTime.Business.EconomyData
{
    public interface IOvertimePayoutStorage
    {
        OvertimePayoutResponsDto DeleteOvertimePayout(int userId, int paidOvertimeId);
        void SaveOvertimePayout(RegisterOvertimePayoutDto overtimePayout);
    }
}
