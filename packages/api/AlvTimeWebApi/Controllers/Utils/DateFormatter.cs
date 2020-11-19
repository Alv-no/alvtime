using System;
using System.Globalization;

namespace AlvTimeWebApi.Controllers.Utils
{
    static class DateFormatter
    {
        public static string ToDateOnly(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
