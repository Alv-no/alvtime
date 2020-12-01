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
        public void GetFlexhours_Worked5Hours_Minus2AndAHalfHourInFlex()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskid));

            _context.Hours.Add(new Hours
            {
                Date = new DateTime(2020, 01, 02),
                Task = new Task { Id = 18 },
                User = 1,
                Value = 2.5M
            });

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(-2.5M, flexhours.TotalHours);
        }

        [Fact]
        public void GetFlexedhours_Flexed2AndAHalfHours_2AndAHalfHoursFlexed()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskid));

            _context.Hours.Add(new Hours
            {
                Date = new DateTime(2020, 01, 02),
                TaskId = 18,
                User = 1,
                Value = 2.5M
            });

            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetFlexedHours(1);

            Assert.Equal(2.5M, flexhours.TotalHours);
        }

        [Fact]
        public void GetFlexhoursToday_YouWorked10YesterdayAnd10HoursToday_5HoursInFlex()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 10M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetAvailableHours(1);

            Assert.Equal(5.0M, flexhours.TotalHours);
        }

        [Fact]
        public void GetEntriesbyDate_Worked2TasksSameDay_2Entries()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskid));

            _context.Hours.Add(new Hours
            {
                Date = new DateTime(2020, 01, 02),
                Task = new Task { Id = 18 },
                User = 1,
                Value = 2.5M
            });
            _context.CompensationRate.Add(CreateCompensationRate(18, 1.0M));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            var calculator = CreateStorage();
            var flexhours = calculator.GetTimeEntries(new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1);

            Assert.Equal(2, flexhours.First().Entries.Count());
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
