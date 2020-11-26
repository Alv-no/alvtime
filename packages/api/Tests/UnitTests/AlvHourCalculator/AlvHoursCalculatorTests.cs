using AlvTime.Business;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.UnitTests.AlvHourCalculator
{
    public class AlvHoursCalculatorTests
    {
        [Fact]
        public void CalculateDefaultFlexiHours_ChristmasOnThursdayOrSunday_SixDaysTotal()
        {
            var calculator = new AlvHoursCalculator();

            var hoursWhenChristmasOnThursday = calculator.CalculateDefaultAlvHours(DayOfWeek.Thursday);
            var hoursWhenChristmasOnSunday = calculator.CalculateDefaultAlvHours(DayOfWeek.Sunday);

            Assert.True(45.0M == hoursWhenChristmasOnThursday && 
                hoursWhenChristmasOnThursday == hoursWhenChristmasOnSunday);
        }

        [Fact]
        public void CalculateDefaultFlexiHours_ChristmasOnFridayOrSaturday_SevenDaysTotal()
        {
            var calculator = new AlvHoursCalculator();

            var hoursWhenChristmasOnFriday = calculator.CalculateDefaultAlvHours(DayOfWeek.Friday);
            var hoursWhenChristmasOnSaturday = calculator.CalculateDefaultAlvHours(DayOfWeek.Saturday);

            Assert.True(52.5M == hoursWhenChristmasOnFriday &&
                hoursWhenChristmasOnFriday == hoursWhenChristmasOnSaturday);
        }

        [Fact]
        public void CalculateDefaultFlexiHours_ChristmasOnMondayTuesdayOrWednesday_FiveDaysTotal()
        {
            var calculator = new AlvHoursCalculator();

            var hoursWhenChristmasOnMonday = calculator.CalculateDefaultAlvHours(DayOfWeek.Monday);
            var hoursWhenChristmasOnTuesday = calculator.CalculateDefaultAlvHours(DayOfWeek.Tuesday);
            var hoursWhenChristmasOnWednesday = calculator.CalculateDefaultAlvHours(DayOfWeek.Wednesday);

            Assert.True(37.5M == hoursWhenChristmasOnMonday &&
                hoursWhenChristmasOnMonday == hoursWhenChristmasOnTuesday &&
                hoursWhenChristmasOnMonday == hoursWhenChristmasOnWednesday);
        }
    }
}
