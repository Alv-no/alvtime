using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.Flexihours
{
    public class GetFlexihoursTests
    {
        private AlvTime_dbContext _context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

        [Fact]
        public void GetFlexhours_NoWorkAtAll_Minus1WorkDayInFlexhour()
        {
            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == -7.5M);
        }

        [Fact]
        public void GetFlexhours_NoWorkFor2Days_Minus2WorkDaysInFlexhour()
        {
            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);

            Assert.True(flexhours.Sum(item => item.Value) == -15.0M);
        }

        [Fact]
        public void GetFlexhours_NoWorkFor2DaysAndRecorded0ForOneOfThem_Minus2WorkDaysInFlexhour()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 0M));
            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);

            Assert.True(flexhours.Sum(item => item.Value) == -15.0M);
        }

        [Fact]
        public void GetFlexhours_NormalWorkday_NoFlexhour()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 7.5M));
            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Empty(flexhours);
        }

        [Fact]
        public void GetFlexhours_Worked10Hours_2AndAHalfHourInFlex()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == 2.5M);
        }

        [Fact]
        public void GetFlexhours_Worked5Hours_Minus2AndAHalfHourInFlex()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 5M));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == -2.5M);
        }

        [Fact]
        public void GetFlexhours_Worked10HoursAnd10Hours_5HourInFlex()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M));
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);

            Assert.Contains(flexhours, hour => hour.Value == 2.5M && hour.Date == new DateTime(2020, 01, 01));
            Assert.Contains(flexhours, hour => hour.Value == 2.5M && hour.Date == new DateTime(2020, 01, 02));
        }

        [Fact]
        public void GetFlexhours_Recorded0HoursAnd7AndAHalfHoursSameDay_NoFlex()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 0M));
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 7.5M));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Empty(flexhours);
        }

        [Fact]
        public void GetFlexhours_YouWorked10HoursAndSomeoneElseWorked8Hours_2AndAHalfHoursInFlex()
        {
            Hours entry1 = CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M);
            entry1.User = 1;
            _context.Hours.Add(entry1);

            Hours entry2 = CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 8M);
            entry2.User = 2;
            _context.Hours.Add(entry2);
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == 2.5M);
        }

        [Fact]
        public void GetFlexhoursToday_YouWorked10YesterdayAnd10HoursToday_2AndAHalfHoursInFlex()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M));
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2019, 12, 31), value: 10M));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Single(flexhours);
            Assert.Contains(flexhours, hour => hour.Value == 2.5M);
        }

        [Fact]
        public void GetFlexhoursToday_YouWorked10YesterdayAnd10HoursToday_5HoursInFlex()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M));
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2019, 12, 31), value: 10M));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2019, 12, 31), new DateTime(2020, 01, 01), 1);

            Assert.Equal(5.0M, flexhours.Sum(item => item.Value));
        }

        [Fact]
        public void GetFlexhours_Worked15HoursOneDayAndTakeOneDayOff_NoFlexHours()
        {
            Hours entry1 = CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 15M);
            entry1.Task.Id = 13;
            _context.Hours.Add(entry1);

            Hours entry2 = CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 7.5M);
            entry2.Task.Id = 14;
            _context.Hours.Add(entry2);

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);

            Assert.Equal(2, flexhours.Count());
            Assert.Equal(7.5M, flexhours.First().Value);
            Assert.Equal(-7.5M, flexhours.Last().Value);
            Assert.Equal(0M, flexhours.Sum(item => item.Value));
        }

        [Fact]
        public void GetFlexhours_Worked10HoursAnd1HourFlexSameDay_2AndAHalfHourFlex()
        {
            Hours entry1 = CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M);
            entry1.Task.Id = 13;
            _context.Hours.Add(entry1);

            Hours entry2 = CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 1M);
            entry2.Task.Id = 14;
            _context.Hours.Add(entry2);

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Equal(2.5M, flexhours.Single().Value);
        }

        [Fact]
        public void GetFlexhours_Recorded0HoursOnSaturday_EmptyFlexHourList()
        {
            Hours entry1 = CreateTimeEntry(date: new DateTime(2020, 09, 12), value: 0M);
            entry1.Task.Id = 1;
            _context.Hours.Add(entry1);

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 09, 12), new DateTime(2020, 09, 12), 1);

            Assert.Empty(flexhours);
        }

        [Fact]
        public void GetFlexhours_Recorded0HoursOnSunday_EmptyFlexHourList()
        {
            Hours entry1 = CreateTimeEntry(date: new DateTime(2020, 09, 13), value: 0M);
            entry1.Task.Id = 1;
            _context.Hours.Add(entry1);

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 09, 13), new DateTime(2020, 09, 13), 1);

            Assert.Empty(flexhours);
        }

        private FlexhourStorage CreateStorage()
        {
            return new FlexhourStorage(new TimeEntryStorage(_context), _context);
        }

        private static Hours CreateTimeEntry(DateTime date, decimal value)
        {
            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                Task = new Task { }
            };
        }
    }
}
