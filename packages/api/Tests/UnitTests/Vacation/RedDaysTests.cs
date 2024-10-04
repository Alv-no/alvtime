using System;
using AlvTime.Business.Absence;
using Xunit;

namespace Tests.UnitTests.Vacation;

public class RedDaysTests
{
    [Fact]
    public void RedDays_YearIs2020_ListHasCorrectRedDays()
    {
        var redDays = new RedDays(2020);

        Assert.Contains(new DateTime(2020, 01, 01), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2020, 04, 04), redDays.Dates);
        Assert.Contains(new DateTime(2020, 04, 05), redDays.Dates);
        Assert.Contains(new DateTime(2020, 04, 06), redDays.Dates);
        Assert.Contains(new DateTime(2020, 04, 07), redDays.Dates);
        Assert.Contains(new DateTime(2020, 04, 08), redDays.Dates);
        Assert.Contains(new DateTime(2020, 04, 09), redDays.Dates);
        Assert.Contains(new DateTime(2020, 04, 10), redDays.Dates);
        Assert.Contains(new DateTime(2020, 04, 11), redDays.Dates);
        Assert.Contains(new DateTime(2020, 04, 12), redDays.Dates);
        Assert.Contains(new DateTime(2020, 04, 13), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2020, 04, 14), redDays.Dates);
        Assert.Contains(new DateTime(2020, 05, 01), redDays.Dates);
        Assert.Contains(new DateTime(2020, 05, 17), redDays.Dates);
        Assert.Contains(new DateTime(2020, 05, 21), redDays.Dates);
        Assert.Contains(new DateTime(2020, 05, 31), redDays.Dates);
        Assert.Contains(new DateTime(2020, 06, 01), redDays.Dates);
        Assert.Contains(new DateTime(2020, 12, 24), redDays.Dates);
        Assert.Contains(new DateTime(2020, 12, 25), redDays.Dates);
        Assert.Contains(new DateTime(2020, 12, 26), redDays.Dates);
        Assert.Contains(new DateTime(2020, 12, 31), redDays.Dates);
    }

    [Fact]
    public void RedDays_YearIs2019_ListHasCorrectRedDays()
    {
        var redDays = new RedDays(2019);

        Assert.Contains(new DateTime(2019, 04, 14), redDays.Dates);
        Assert.Contains(new DateTime(2019, 04, 17), redDays.Dates);
        Assert.Contains(new DateTime(2019, 04, 22), redDays.Dates);
        Assert.Contains(new DateTime(2019, 04, 19), redDays.Dates);
        Assert.Contains(new DateTime(2019, 05, 30), redDays.Dates);
        Assert.Contains(new DateTime(2019, 06, 09), redDays.Dates);
        Assert.Contains(new DateTime(2019, 06, 10), redDays.Dates);
    }

    [Fact]
    public void RedDays_YearIs2018_ListHasCorrectRedDays()
    {
        var redDays = new RedDays(2018);

        Assert.Contains(new DateTime(2018, 01, 01), redDays.Dates);
        Assert.Contains(new DateTime(2018, 03, 25), redDays.Dates);
        Assert.Contains(new DateTime(2018, 03, 26), redDays.Dates);
        Assert.Contains(new DateTime(2018, 04, 02), redDays.Dates);
        Assert.Contains(new DateTime(2018, 05, 17), redDays.Dates);
        Assert.Contains(new DateTime(2018, 05, 20), redDays.Dates);
        Assert.Contains(new DateTime(2018, 05, 21), redDays.Dates);
        Assert.Contains(new DateTime(2018, 12, 27), redDays.Dates);
    }

    [Fact]
    public void RedDays_YearIs2021_ListHasCorrectRedDays()
    {
        var redDays = new RedDays(2021);

        Assert.Contains(new DateTime(2021, 03, 28), redDays.Dates);
        Assert.Contains(new DateTime(2021, 03, 30), redDays.Dates);
        Assert.Contains(new DateTime(2021, 03, 31), redDays.Dates);
        Assert.Contains(new DateTime(2021, 04, 05), redDays.Dates);
        Assert.Contains(new DateTime(2021, 05, 13), redDays.Dates);
        Assert.Contains(new DateTime(2021, 05, 23), redDays.Dates);
        Assert.Contains(new DateTime(2021, 05, 24), redDays.Dates);
    }

    [Fact]
    public void RedDays_YearIs2022_ListHasCorrectRedDays()
    {
        var redDays = new RedDays(2022);

        Assert.Contains(new DateTime(2022, 04, 10), redDays.Dates);
        Assert.Contains(new DateTime(2022, 04, 15), redDays.Dates);
        Assert.Contains(new DateTime(2022, 04, 18), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2022, 04, 19), redDays.Dates);
        Assert.Contains(new DateTime(2022, 06, 05), redDays.Dates);
        Assert.Contains(new DateTime(2022, 06, 06), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2022, 04, 11), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2022, 04, 12), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2022, 04, 13), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2022, 06, 04), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2022, 12, 27), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2022, 12, 30), redDays.Dates);
    }

    [Fact]
    public void RedDays_YearIs2030_ListHasCorrectRedDays()
    {
        var redDays = new RedDays(2030);

        Assert.Contains(new DateTime(2030, 04, 21), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2030, 04, 15), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2030, 04, 16), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2030, 04, 17), redDays.Dates);
        Assert.Contains(new DateTime(2030, 04, 22), redDays.Dates);
        Assert.Contains(new DateTime(2030, 06, 09), redDays.Dates);
        Assert.Contains(new DateTime(2030, 06, 10), redDays.Dates);
        Assert.Contains(new DateTime(2030, 12, 24), redDays.Dates);
        Assert.Contains(new DateTime(2030, 12, 31), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2030, 12, 23), redDays.Dates);
    }

    [Fact]
    public void RedDays_YearIs2010_ListHasCorrectRedDays()
    {
        var redDays = new RedDays(2010);

        Assert.Contains(new DateTime(2010, 04, 01), redDays.Dates);
        Assert.Contains(new DateTime(2010, 04, 02), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2010, 04, 06), redDays.Dates);
        Assert.Contains(new DateTime(2010, 05, 01), redDays.Dates);
        Assert.Contains(new DateTime(2010, 05, 24), redDays.Dates);
        Assert.DoesNotContain(new DateTime(2010, 05, 22), redDays.Dates);
    }

    [Fact]
    public void RedDays_YearIs2000_ListHasCorrectRedDays()
    {
        var redDays = new RedDays(2000);

        Assert.Contains(new DateTime(2000, 01, 01), redDays.Dates);
        Assert.Contains(new DateTime(2000, 04, 16), redDays.Dates);
        Assert.Contains(new DateTime(2000, 06, 01), redDays.Dates);
        Assert.Contains(new DateTime(2000, 06, 11), redDays.Dates);
    }
}