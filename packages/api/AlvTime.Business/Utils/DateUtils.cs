using System;
using System.Collections.Generic;

namespace AlvTime.Business.Utils;

public static class DateUtils
{
    public static IEnumerable<DateTime> GetWeekendDays(DateTime fromDate, DateTime toDate)
    {
        var nextWeekendDay = DaysTilNearestWeekendDay(fromDate);
        var currentDate = fromDate.AddDays(nextWeekendDay);

        while (currentDate <= toDate)
        {
            yield return currentDate;
            currentDate = currentDate.AddDays(1);
            nextWeekendDay = DaysTilNearestWeekendDay(currentDate);
            currentDate = currentDate.AddDays(nextWeekendDay);
        }
    }

    private static int DaysTilNearestWeekendDay(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            return 0;
        }

        return 6 - (int)date.DayOfWeek;
    }
    
    public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
    {
        for(var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            yield return day;
    }
}
