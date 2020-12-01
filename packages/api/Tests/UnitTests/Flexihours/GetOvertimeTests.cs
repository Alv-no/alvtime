using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using AlvTime.Business.FlexiHours;
using Xunit;

namespace Tests.UnitTests.Flexihours
{
    public class GetOvertimeTests
    {
        private readonly AlvTime_dbContext _context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

        private readonly DateTime _startDate = new DateTime(2020, 01, 02);

        [Fact]
        public void GetFlexhours_NoWorkAtAll_AvailableIs0Overtime0Flex()
        {
            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(0, flexhours.TotalHours);
            Assert.Equal(0, flexhours.TotalHoursIncludingCompensationRate);
        }

        [Fact]
        public void GetFlexhours_NormalWorkday_NoFlexhour()
        {
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 7.5M, out int taskid));
            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(0, flexhours.TotalHours);
            Assert.Equal(0, flexhours.TotalHoursIncludingCompensationRate);
        }

        [Fact]
        public void GetFlexhours_WorkedOvertime_PositiveFlexAndOvertime()
        {
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 10M, out int taskid));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(2.5M, flexhours.TotalHours);
            Assert.Equal(2.5M, flexhours.TotalHoursIncludingCompensationRate);
        }

        [Fact]
        public void GetFlexhours_ExhangedHoursIntoPayout_AvailableHoursAreCompensatedForPayout()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 15M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 2.0M)); // 15 hours overtime, 7.5 hours to flex
            _context.SaveChanges();

            var calculator = CreateStorage();
            calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 3M
            }, userId: 1);

            // 4.5 hours to flex, 3 * 2 = 6h. 15 - 6 = 9 hours available overtime.
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(4.5M, flexhours.TotalHours);
            Assert.Equal(9M, flexhours.TotalHoursIncludingCompensationRate);
        }

        [Fact]
        public void GetFlexedhours_WorkedLessThanFullDay_NegativeFlex()
        {
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 5M, out int taskid));
            _context.Hours.Add(CreateFlexEntry(date: _startDate, value: 2.5M));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(-2.5M, flexhours.TotalHours);
            Assert.Equal(0, flexhours.TotalHoursIncludingCompensationRate);
        }

        [Fact]
        public void GetFlexhours_WorkedMultipleDays_FlexForMultipleDaysAreSummed()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskid));
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 10M, out int taskid2));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(5, flexhours.TotalHours);
            Assert.Equal(5, flexhours.TotalHoursIncludingCompensationRate);
        }

        [Fact]
        public void GetFlexhours_MultipleEmployees_FlexForSpecifiedEmployeeIsCalculated()
        {
            var entry1 = CreateTimeEntry(date: _startDate, value: 10M, out int taskid);
            entry1.User = 1;
            _context.Hours.Add(entry1);

            var entry2 = CreateTimeEntry(date: _startDate, value: 8M, out int taskid2);
            entry2.User = 2;
            _context.Hours.Add(entry2);
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(userId: 1);
         
            Assert.Equal(2.5M, flexhours.TotalHours);
            Assert.Equal(2.5M, flexhours.TotalHoursIncludingCompensationRate);
        }

        [Fact]
        public void GetFlexhoursToday_EntriesBeforeStartDate_NotTakenIntoAccount()
        {
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 10M, out int taskid));
            _context.Hours.Add(CreateTimeEntry(date: _startDate.AddDays(-1), value: 10M, out int taskid2));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(2.5M, flexhours.TotalHours);
            Assert.Equal(2.5M, flexhours.TotalHoursIncludingCompensationRate);
        }

        [Fact]
        public void GetFlexhours_NotWorkedInWeekend_NoImpactOnOverTimeNorFlex()
        {
            // saturday and sunday:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 04), value: 0M, out _));
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 05), value: 0M, out _));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(0, flexhours.TotalHours);
            Assert.Equal(0, flexhours.TotalHours);
        }

        [Fact]
        public void GetEntriesbyDate_MultipleTasks_FlexhoursCompensatesForMultipleTasks()
        {
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 5M, out _));
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 2.5M, out _));
            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(0, flexhours.TotalHours);
            Assert.Equal(0, flexhours.TotalHoursIncludingCompensationRate);
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

        private static Hours CreateFlexEntry(DateTime date, decimal value)
        {
            return new Hours
            {
                Date = date,
                Task = new Task { Id = 18 },
                TaskId = 18,
                User = 1,
                Value = value
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
