using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTime.Business.Holidays
{
    public class RedDaysService : IRedDaysService
    {
        public List<string> GetRedDaysFromYear(int year)
        {
            var dates = new List<string>();

                var redDays = new RedDays(year);

                foreach (var date in redDays.Dates)
                {
                    dates.Add(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                }

            return dates;
        }

        public List<string> GetRedDaysFromYears(int fromYearInclusive, int toYearInclusive)
        {
            var dates = new List<string>();

                var yearsToInclude = toYearInclusive - fromYearInclusive + 1;
                var years = Enumerable.Range(fromYearInclusive, yearsToInclude);

                foreach (var yearToFindDays in years)
                {
                    var redDays = new RedDays(yearToFindDays);

                    foreach (var date in redDays.Dates)
                    {
                        dates.Add(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    }
            }

            return dates;
        }
    }
}
