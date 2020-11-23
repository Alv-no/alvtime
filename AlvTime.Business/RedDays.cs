using System;
using System.Collections.Generic;

namespace AlvTime.Business
{
    public class RedDays
    {
        public List<DateTime> Dates { get; set; }

        public RedDays(int year)
        {
            Dates = new List<DateTime>
            {
                new DateTime(year, 01, 01),
                EasterSunday(year).AddDays(-7),
                EasterSunday(year).AddDays(-6),
                EasterSunday(year).AddDays(-5),
                EasterSunday(year).AddDays(-4),
                EasterSunday(year).AddDays(-3),
                EasterSunday(year).AddDays(-2),
                EasterSunday(year).AddDays(-1),
                EasterSunday(year),
                EasterSunday(year).AddDays(1),
                EasterSunday(year).AddDays(39),
                EasterSunday(year).AddDays(49),
                EasterSunday(year).AddDays(50),
                new DateTime(year, 05, 01),
                new DateTime(year, 05, 17),
                new DateTime(year, 12, 24),
                new DateTime(year, 12, 25),
                new DateTime(year, 12, 26),
                new DateTime(year, 12, 27),
                new DateTime(year, 12, 28),
                new DateTime(year, 12, 29),
                new DateTime(year, 12, 30),
                new DateTime(year, 12, 31),

            };
        }

        public DateTime EasterSunday(int year)
        {
            int g = year % 19;
            int c = year / 100;
            int h = (c - c / 4 - (8 * c + 13) / 25 + 19 * g + 15) % 30;
            int i = h - h / 28 * (1 - h / 28 * (29 / (h + 1)) * ((21 - g) / 11));

            int day = i - (year + year / 4 + i + 2 - c + c / 4) % 7 + 28;
            int month = 3;
            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }
    }
}
