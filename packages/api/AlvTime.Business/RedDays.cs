using System;
using System.Collections.Generic;

namespace AlvTime.Business
{
    public class RedDays
    {
        public List<DateTime> Dates { get; set; }

        public RedDays(int year)
        {
            if (year < 2022)
            {
                Dates = new List<DateTime>
                {
                    new DateTime(year, 01, 01).Date,
                    GetPalmeSondag(year).Date,
                    GetMondayInEaster(year).Date,
                    GetTuesdayInEaster(year).Date,
                    GetWednesdayInEaster(year).Date,
                    GetSkjaerTorsdag(year).Date,
                    GetLangfredag(year).Date,
                    GetPaskeAften(year).Date,
                    GetEasterSunday(year).Date,
                    GetAndrePaskeDag(year).Date,
                    GetKristiHimmelfart(year).Date,
                    GetForstePinsedag(year).Date,
                    GetAndrePinsedag(year).Date,
                    new DateTime(year, 05, 01).Date,
                    new DateTime(year, 05, 17).Date,
                    new DateTime(year, 12, 24).Date,
                    new DateTime(year, 12, 25).Date,
                    new DateTime(year, 12, 26).Date,
                    new DateTime(year, 12, 27).Date,
                    new DateTime(year, 12, 28).Date,
                    new DateTime(year, 12, 29).Date,
                    new DateTime(year, 12, 30).Date,
                    new DateTime(year, 12, 31).Date,
                };
            }
            else
            {
                Dates = new List<DateTime>
                {
                    new DateTime(year, 01, 01).Date,
                    GetPalmeSondag(year).Date,
                    GetSkjaerTorsdag(year).Date,
                    GetLangfredag(year).Date,
                    GetPaskeAften(year).Date,
                    GetEasterSunday(year).Date,
                    GetAndrePaskeDag(year).Date,
                    GetKristiHimmelfart(year).Date,
                    GetForstePinsedag(year).Date,
                    GetAndrePinsedag(year).Date,
                    new DateTime(year, 05, 01).Date,
                    new DateTime(year, 05, 17).Date,
                    new DateTime(year, 12, 24).Date,
                    new DateTime(year, 12, 25).Date,
                    new DateTime(year, 12, 26).Date,
                    new DateTime(year, 12, 31).Date,
                };
            }
            
        }

        public DateTime GetPalmeSondag(int year)
        {
            return GetEasterSunday(year).AddDays(-7);
        }

        public DateTime GetMondayInEaster(int year)
        {
            return GetEasterSunday(year).AddDays(-6);
        }
        public DateTime GetTuesdayInEaster(int year)
        {
            return GetEasterSunday(year).AddDays(-5);
        }

        public DateTime GetWednesdayInEaster(int year)
        {
            return GetEasterSunday(year).AddDays(-4);
        }

        public DateTime GetSkjaerTorsdag(int year)
        {
            return GetEasterSunday(year).AddDays(-3);
        }

        public DateTime GetLangfredag(int year)
        {
            return GetEasterSunday(year).AddDays(-2);
        }

        public DateTime GetPaskeAften(int year)
        {
            return GetEasterSunday(year).AddDays(-1);
        }

        public DateTime GetAndrePaskeDag(int year)
        {
            return GetEasterSunday(year).AddDays(1);
        }
        public DateTime GetKristiHimmelfart(int year)
        {
            return GetEasterSunday(year).AddDays(39);
        }
        public DateTime GetForstePinsedag(int year)
        {
            return GetEasterSunday(year).AddDays(49);
        }
        public DateTime GetAndrePinsedag(int year)
        {
            return GetEasterSunday(year).AddDays(50);
        }

        // This calculation was found in a StackOverflow post: 
        // https://stackoverflow.com/questions/2510383/how-can-i-calculate-what-date-good-friday-falls-on-given-a-year
        public DateTime GetEasterSunday(int year)
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
