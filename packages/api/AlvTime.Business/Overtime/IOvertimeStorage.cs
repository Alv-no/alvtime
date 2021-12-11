using System;
using AlvTime.Business.TimeEntries;

namespace AlvTime.Business.Overtime
{
    public interface IOvertimeStorage
    {
        void StoreOvertime(DateTime timeEntryDate, int userId);
    }
}