using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;

namespace AlvTime.Business.EconomyData
{
    public interface IOvertimePayoutStorage
    {
        OvertimePayoutResponsDto DeleteOvertimePayout(int userId, DateTime date);
        void SaveOvertimePayout(RegisterOvertimePayoutDto overtimePayout);
        List<OvertimeEntry> GetOvertimeEntriesForPayout(List<OvertimeEntry> overtimeEntries, decimal hoursForPayout);
    }
}
