using System.Collections.Generic;

namespace AlvTime.Business.Holidays
{
    public interface IRedDaysService
    {
        List<string> GetRedDays(int year, int fromYearInclusive, int toYearInclusive);
    }
}
