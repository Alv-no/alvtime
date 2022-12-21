using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Utils;
using Xunit;

namespace Tests.UnitTests.BusinessLogic;

public class DateUtilTests
{
    [Fact]
    public void GetsWeekendDays_When_Whole_Week()
    {
        var weekDays = DateUtils.GetWeekendDays(new DateTime(2022, 12, 19), new DateTime(2022, 12, 25));
        Assert.Equal(2, weekDays.Count());

        Assert.Equal(DayOfWeek.Saturday, weekDays.ElementAt(0).DayOfWeek);
        Assert.Equal(DayOfWeek.Sunday, weekDays.ElementAt(1).DayOfWeek);
        AllDatesAreSaturdayOrSunday(weekDays);
    }

    [Fact]
    public void GetsWeekendDay_For_One_Month()
    {
        var weekDays = DateUtils.GetWeekendDays(new DateTime(2022, 12, 1), new DateTime(2022, 12, 31));
        Assert.Equal(9, weekDays.Count());
        AllDatesAreSaturdayOrSunday(weekDays);
    }

    [Fact]
    public void GetsWeekendDay_For_One_Month_Starting_Sunday()
    {
        var weekDays = DateUtils.GetWeekendDays(new DateTime(2022, 12, 04), new DateTime(2022, 12, 31));
        Assert.Equal(8, weekDays.Count());
        AllDatesAreSaturdayOrSunday(weekDays);
    }

    [Fact]
    public void GetsWeekendDay_For_One_Month_Starting_Saturday()
    {
        var weekDays = DateUtils.GetWeekendDays(new DateTime(2022, 12, 03), new DateTime(2022, 12, 31));
        Assert.Equal(9, weekDays.Count());
        AllDatesAreSaturdayOrSunday(weekDays);
    }

    private void AllDatesAreSaturdayOrSunday(IEnumerable<DateTime> dates)
    {
        foreach (var date in dates)
        {
            Assert.True(date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
        }
    }
}
