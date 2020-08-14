using System;
using System.Collections.Generic;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexhourCalculator
    {
        IEnumerable<FlexiHours> GetFlexihours(DateTime dateTime, DateTime dateTime1, int userId);
    }
}
