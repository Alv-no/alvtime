using System;
using System.Collections.Generic;
using System.Text;

namespace AlvTime.Business.FlexiHours
{
    public interface IFlexiHourStorage
    {
        IEnumerable<FlexiHoursResponseDto> GetFlexiHours(int userId, DateTime startDate, DateTime endDate);
    }
}
