using System;
using System.Collections.Generic;
using System.Text;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexiHourStorage
    {
        FlexiHourResponseDto GetTotalFlexiHours();
        FlexiHourResponseDto GetUsedFlexiHours(int userId);
    }
}
