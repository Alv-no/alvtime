using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using FluentValidation;
using Xunit;
using static Tests.UnitTests.Flexihours.GetOvertimeTests;

namespace Tests.UnitTests.Flexihours
{
    public class RegisterPaidOvertimeTests
    {
        private AlvTime_dbContext _context = new AlvTimeDbContextBuilder()
        .WithUsers()
        .CreateDbContext();

        [Fact]
        public void GetRegisteredPayouts_Registered10Hours_10HoursRegistered()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 1, 2),
                Hours = 10
            }, 1);

            var registeredPayouts = calculator.GetRegisteredPayouts(1);

            Assert.Equal(10, registerOvertimeResponse.Value);
            Assert.Equal(10, registeredPayouts.TotalHours);
        }

        [Fact]
        public void GetRegisteredPayouts_Registered3Times_ListWith5Items()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 3
            }, 1);
            calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 3
            }, 1);
            calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 4
            }, 1);

            var registeredPayouts = calculator.GetRegisteredPayouts(1);

            Assert.Equal(3, registeredPayouts.Entries.Count());
        }

        [Fact]
        public void RegisterPayout_CalculationCorrectForBeforeAndAfterCompRate()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 2.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 17.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 0.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var registeredPayout = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 03),
                Hours = 10
            }, 1).ValueAfterCompRate;

            Assert.Equal(5, registeredPayout);
        }

        [Fact]
        public void RegisterPayout_CalculationCorrectForBeforeAndAfterCompRate2()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 8.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 12.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 0.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 9M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 07), value: 9.5M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var registeredPayout = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 07),
                Hours = 6
            }, 1);

            Assert.Equal(3.5M, registeredPayout.ValueAfterCompRate);
            Assert.Equal(6M, registeredPayout.Value);
        }

        [Fact]
        public void RegisterPayout_NotEnoughOvertime_CannotRegisterPayout()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 11.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            Assert.Throws<ValidationException>(() => calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 7
            }, 1);

            Assert.Equal(null, result);
        }

        [Fact]
        public void RegisterPayout_RegisteringPayoutBeforeWorkingOvertime_NoPayout()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 11.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            Assert.Throws<ValidationException>(() => calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 1
            }, 1);

            Assert.Equal(null, result);
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
