using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;

namespace AlvTime.Business.EconomyData
{
    public interface IOvertimePayoutStorage
    {
        OvertimePayoutResponsDto DeleteOvertimePayout(int userId, DateTime date);
        void RegisterTotalOvertimePayout(RegisterOvertimePayoutDto overtimePayout);

        decimal RegisterOvertimePayoutSalary(List<OvertimeEntry> overtimeEntries, int userId,
            GenericHourEntry requestedPayout);
    }
}
