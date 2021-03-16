using AlvTime.Business.Helpers;
using AlvTime.Business.TimeEntries;
using System.Collections.Generic;
using Xunit;

namespace Tests.UnitTests.BusinessLogic
{
    public class VacationExtensionTest
    {
        [Fact]
        public void CalculateVacationOverView_ListOfTimeEntries_CalculatesCorrectOverview()
        {
            var timeEntries = new List<TimeEntriesResponseDto>
            {
                {
                    new TimeEntriesResponseDto
                    {
                        Value = 7.5M
                    }
                },
                {
                    new TimeEntriesResponseDto
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
        public void CalculateVacationOverView_ListOfTimeEntries_CalculatesCorrectOverview2()
        {
            var timeEntries = new List<TimeEntriesResponseDto>
            {
                {
                    new TimeEntriesResponseDto
                    {
                        Value = 7.5M
                    }
                },
                {
                    new TimeEntriesResponseDto
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
}
