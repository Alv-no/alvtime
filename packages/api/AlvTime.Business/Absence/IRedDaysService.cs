using System.Collections.Generic;

namespace AlvTime.Business.Absence
{
    public interface IRedDaysService
    {
        List<string> GetRedDaysFromYear(int year);
        List<string> GetRedDaysFromYears(int fromYearInclusive, int toYearInclusive);
    }
}
