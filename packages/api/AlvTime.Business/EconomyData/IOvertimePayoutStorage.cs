using System;

namespace AlvTime.Business.EconomyData
{
    public interface IOvertimePayoutStorage
    {
        OvertimePayoutResponsDto DeleteOvertimePayout(int userId, DateTime date);
        void RegisterTotalOvertimePayout(RegisterOvertimePayoutDto overtimePayout);
    }
}
