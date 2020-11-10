using AlvTime.Business.FlexiHours;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.Flexihours
{
    public class RegisterPaidOvertimeTests
    {
        private AlvTime_dbContext _context = new AlvTimeDbContextBuilder()
        .WithUsers()
        .CreateDbContext();

        [Fact]
        public void RegisterpaidOvertime_10HoursAvailable_AbleToRegister10Hours()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new RegisterPaidOvertimeDto
            {
                Date = new DateTime(2020, 01, 01),
                Value = 10
            }, 1);

            Assert.Equal(10, registerOvertimeResponse.Value);
            Assert.Contains(flexhours, hour => hour.Value == 10M);
        }

        [Fact]
        public void RegisterpaidOvertime_10HoursAvailable_UnAbleToRegister11Hours()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new RegisterPaidOvertimeDto
            {
                Date = new DateTime(2020, 01, 01),
                Value = 11
            }, 1);

            Assert.Equal(0, registerOvertimeResponse.Value);
            Assert.Contains(flexhours, hour => hour.Value == 10M);
        }

        [Fact]
        public void GetRegisteredPayouts_Registered10Hours_10HoursRegistered()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new RegisterPaidOvertimeDto
            {
                Date = new DateTime(2020, 01, 01),
                Value = 10
            }, 1);

            var registeredPayouts = calculator.GetRegisteredPayouts(new DateTime(2020, 01, 01), new DateTime(2020, 12, 31), 1);

            Assert.Equal(10, registerOvertimeResponse.Value);
            Assert.Equal(10, registeredPayouts.First().Value);
        }

        [Fact]
        public void GetRegisteredPayouts_Registered3Times_ListWith5Items()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            calculator.RegisterPaidOvertime(new RegisterPaidOvertimeDto
            {
                Date = new DateTime(2020, 01, 01),
                Value = 3
            }, 1);
            calculator.RegisterPaidOvertime(new RegisterPaidOvertimeDto
            {
                Date = new DateTime(2020, 01, 01),
                Value = 3
            }, 1);
            calculator.RegisterPaidOvertime(new RegisterPaidOvertimeDto
            {
                Date = new DateTime(2020, 01, 01),
                Value = 4
            }, 1);

            var registeredPayouts = calculator.GetRegisteredPayouts(new DateTime(2020, 01, 01), new DateTime(2020, 12, 31), 1);

            Assert.Equal(3, registeredPayouts.Count());
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
