using System;
using System.Collections.Generic;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexhourRepository
    {
        IEnumerable<FlexHours> GetFlexhours(DateTime dateTime, DateTime dateTime1, int userId);
    }
}
