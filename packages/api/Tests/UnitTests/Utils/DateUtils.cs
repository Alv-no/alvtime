using System;

namespace Tests.UnitTests.Utils;

public static class DateUtils
{

    public static DateTime GetFutureNonWeekendDay()
    {
        var currentYear = DateTime.Now.AddYears(1).Year;

        var futureDate = new DateTime(currentYear, 06, 21);
        if (futureDate.DayOfWeek == DayOfWeek.Saturday || futureDate.DayOfWeek == DayOfWeek.Sunday)
        {
            futureDate = futureDate.AddDays(3);
        }

        return futureDate;
    }
}
