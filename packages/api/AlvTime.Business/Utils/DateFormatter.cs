using System;
using System.Globalization;

namespace AlvTime.Business.Utils;

public static class DateFormatter
{
    public static string ToDateOnly(this DateTime date)
    {
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}