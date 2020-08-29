using System;
using System.Collections.Generic;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexhourStorage
    {
        IEnumerable<FlexiHours> GetFlexihours(DateTime dateTime, DateTime dateTime1, int userId);
        RegisterPaidOvertimeDto RegisterPaidOvertime(DateTime date, decimal valueRegistered, int userId);
    }
}
