using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using static Tests.UnitTests.Flexihours.GetOvertimeTests;

namespace Tests.UnitTests.Flexihours
{
    public class CancelPayoutTests
    {
        private AlvTime_dbContext _context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();

        [Fact]
        public void CancelPayout_PayoutIsRegisteredInSameMonth_PayoutIsCanceled()
        {
            var dbUser = _context.User.First();
            dbUser.StartDate = new DateTime(2020, 11, 01);

            var currentMonth = DateTime.Now.Month;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, currentMonth, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, currentMonth, 02),
                Hours = 10
            }, 1).Value as PaidOvertime;

            var canceledPayout = calculator.CancelPayout(1, 1);

            Assert.Equal(1, canceledPayout.Id);
        }

        [Fact]
        public void CancelPayout_PayoutIsRegisteredPreviousMonth_PayoutIsLocked()
        {
            var dbUser = _context.User.First();
            dbUser.StartDate = new DateTime(2020, 10, 01);

            var previousMonth = DateTime.Now.AddMonths(-1).Month;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, previousMonth, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, previousMonth, 02),
                Hours = 10
            }, 1).Value as PaidOvertime;

            var canceledPayout = calculator.CancelPayout(1, 1);

            Assert.Equal(0, canceledPayout.Id);
        }

        private FlexhourStorage CreateStorage()
        {
            return new FlexhourStorage(new TimeEntryStorage(_context), _context, new TestTimeEntryOptions(
                new TimeEntryOptions
                {
                    FlexTask = 18,
                    ReportUser = 11,
                    StartOfOvertimeSystem = new DateTime(2020, 01, 01)
                }));
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
