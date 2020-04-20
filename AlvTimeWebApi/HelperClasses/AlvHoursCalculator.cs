using System;

namespace AlvTimeWebApi.HelperClasses
{
    public class AlvHoursCalculator
    {
        public decimal CalculateAlvHours()
        {
            var christmasDayOfWeek = new DateTime(DateTime.UtcNow.Year, 12, 24).DayOfWeek;

            return CalculateDefaultAlvHours(christmasDayOfWeek);
        }

        public decimal CalculateDefaultAlvHours(DayOfWeek christmasDayOfWeek)
        {
            const decimal HOURS_IN_EASTER = 3 * 7.5M;
            const decimal HOURS_IN_WORKDAY = 7.5M;

            var daysBetweenChristmasNewYear = 0;

            switch (christmasDayOfWeek)
            {
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                    daysBetweenChristmasNewYear = 2;
                    break;
                case DayOfWeek.Thursday:
                case DayOfWeek.Sunday:
                    daysBetweenChristmasNewYear = 3;
                    break;
                case DayOfWeek.Friday:
                case DayOfWeek.Saturday:
                    daysBetweenChristmasNewYear = 4;
                    break;
            }

            var alvHours = (daysBetweenChristmasNewYear * HOURS_IN_WORKDAY) + HOURS_IN_EASTER;

            return alvHours;
        }
    }
}
