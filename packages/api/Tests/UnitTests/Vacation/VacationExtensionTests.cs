﻿using System.Collections.Generic;
using AlvTime.Business.Absence;
using AlvTime.Business.TimeRegistration;
using Xunit;

namespace Tests.UnitTests.Vacation;

public class VacationExtensionTests
{
    [Fact]
    public void CalculateVacationOverView_2DaysVacation_OverviewHas2TotalDays0Hours()
    {
        var timeEntries = new List<TimeEntryResponseDto>
        {
            {
                new TimeEntryResponseDto
                {
                    Value = 7.5M
                }
            },
            {
                new TimeEntryResponseDto
                {
                    Value = 7.5M
                }
            }
        };

        var overview = VacationExtension.CalculateVacationOverview(timeEntries);

        Assert.Equal(2, overview.TotalDaysUsed);
        Assert.Equal(0, overview.TotalHoursUsed);
    }

    [Fact]
    public void CalculateVacationOverView_1Day5HoursVacation_OverviewHas1Day5Hours()
    {
        var timeEntries = new List<TimeEntryResponseDto>
        {
            {
                new TimeEntryResponseDto
                {
                    Value = 7.5M
                }
            },
            {
                new TimeEntryResponseDto
                {
                    Value = 5M
                }
            }
        };

        var overview = VacationExtension.CalculateVacationOverview(timeEntries);

        Assert.Equal(1, overview.TotalDaysUsed);
        Assert.Equal(5, overview.TotalHoursUsed);
    }
}