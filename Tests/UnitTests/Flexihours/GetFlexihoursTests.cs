using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
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
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, DateTime.Now, 1);

            Assert.Contains(flexhours, hour => hour.Value == -7.5M);
        }

        [Fact]
        public void GetFlexhours_NoWorkFor2Days_Minus2WorkDaysInFlexhour()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, DateTime.Now, 1);

            Assert.True(flexhours.Sum(item => item.Value) == -15.0M);
        }

        [Fact]
        public void GetFlexhours_NoWorkFor2DaysAndRecorded0ForOneOfThem_Minus2WorkDaysInFlexhour()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 0M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 01), 1);

            Assert.True(flexhours.Sum(item => item.Value) == -15.0M);
        }

        [Fact]
        public void GetFlexhours_NormalWorkday_NoFlexhour()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 01), 1);

            Assert.Empty(flexhours);
        }

        [Fact]
        public void GetFlexhours_Worked10Hours_2AndAHalfHourInFlex()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == 2.5M);
        }

        [Fact]
        public void GetFlexhours_Worked5Hours_Minus2AndAHalfHourInFlex()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == -2.5M);
        }

        [Fact]
        public void GetFlexhours_Worked10HoursAnd10Hours_5HourInFlex()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 02), 1);

            Assert.Contains(flexhours, hour => hour.Value == 2.5M && hour.Date == new DateTime(2020, 01, 01));
            Assert.Contains(flexhours, hour => hour.Value == 2.5M && hour.Date == new DateTime(2020, 01, 02));
        }

        [Fact]
        public void GetFlexhours_Recorded0HoursAnd7AndAHalfHoursSameDay_NoFlex()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 0M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 7.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 01), 1);

            Assert.Empty(flexhours);
        }

        [Fact]
        public void GetFlexhours_YouWorked10HoursAndSomeoneElseWorked8Hours_2AndAHalfHoursInFlex()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            Hours entry1 = CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M, out int taskid);
            entry1.User = 1;
            _context.Hours.Add(entry1);
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            Hours entry2 = CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 8M, out int taskid2);
            entry2.User = 2;
            _context.Hours.Add(entry2);
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == 2.5M);
        }

        [Fact]
        public void GetFlexhoursToday_YouWorked10YesterdayAnd10HoursToday_2AndAHalfHoursInFlex()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2019, 12, 31), value: 10M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 01),  1);

            Assert.Single(flexhours);
            Assert.Contains(flexhours, hour => hour.Value == 2.5M);
        }

        [Fact]
        public void GetFlexhoursToday_YouWorked10YesterdayAnd10HoursToday_5HoursInFlex()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2019, 12, 31), value: 10M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 01), 1);

            Assert.Equal(5.0M, flexhours.Sum(item => item.Value));
        }

        [Fact]
        public void GetFlexhours_Worked15HoursOneDayAndTakeOneDayOff_NoFlexHours()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 15M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 01), 1);

            Assert.Equal(2, flexhours.Count());
            Assert.Equal(7.5M, flexhours.First().Value);
            Assert.Equal(-7.5M, flexhours.Last().Value);
            Assert.Equal(0M, flexhours.Sum(item => item.Value));
        }

        [Fact]
        public void GetFlexhours_Recorded0HoursOnSaturday_EmptyFlexHourList()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            Hours entry1 = CreateTimeEntry(date: new DateTime(2020, 09, 12), value: 0M, out int taskid);
            _context.Hours.Add(entry1);

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 09, 12), 1);

            Assert.Empty(flexhours);
        }

        [Fact]
        public void GetFlexhours_Recorded0HoursOnSunday_EmptyFlexHourList()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            Hours entry1 = CreateTimeEntry(date: new DateTime(2020, 09, 13), value: 0M, out int taskid);
            _context.Hours.Add(entry1);

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 09, 13), 1);

            Assert.Empty(flexhours);
        }

        [Fact]
        public void GetFlexhours_NotRecordedBeforeStarting_EmptyFlexHourList()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 04, 01), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            dbUser.StartDate = new DateTime(2020, 04, 01);

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 04, 01), 1);

            Assert.Empty(flexhours);
        }

        private FlexhourStorage CreateStorage()
        {
            return new FlexhourStorage(new TimeEntryStorage(_context), _context);
        }

        private static Hours CreateTimeEntry(DateTime date, decimal value, out int taskId)
        {
            taskId = new Random().Next();

            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                Task = new Task { Id = taskId }
            };
        }

        private static CompensationRate CreateCompensationRate(int taskId, decimal compRate)
        {
            return new CompensationRate
            {
                FromDate = DateTime.UtcNow,
                Value = compRate,
                TaskId = taskId
            };
        }
    }
}
