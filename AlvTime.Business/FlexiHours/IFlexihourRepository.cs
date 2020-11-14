using System;
using System.Collections.Generic;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexhourStorage
    {
        AvailableHoursDto GetAvailableHours(int userId);
        decimal GetOvertimeEquivalents(DateTime fromDate, DateTime toDate, int userId);
        RegisterPaidOvertimeDto RegisterPaidOvertime(DateTime date, decimal valueRegistered, int userId);
    }
}
