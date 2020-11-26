﻿using AlvTime.Business.FlexiHours;
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
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 02), 1);

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 10
            }, 1).Value as PaidOvertime;

            Assert.Equal(10, registerOvertimeResponse.Value);
            Assert.Contains(flexhours, hour => hour.Value == 10M);
        }

        [Fact]
        public void RegisterpaidOvertime_10HoursAvailable_UnAbleToRegister11Hours()
        {
            var dbUser = _context.User.First();
            var startDate = dbUser.StartDate;

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();
            var flexhours = calculator.GetHoursWorkedMoreThanWorkday(startDate, new DateTime(2020, 01, 02), 1);

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 11
            }, 1).StatusCode;

            Assert.Equal(400, registerOvertimeResponse);
            Assert.Contains(flexhours, hour => hour.Value == 10M);
        }

        [Fact]
        public void GetRegisteredPayouts_Registered10Hours_10HoursRegistered()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 10
            }, 1).Value as PaidOvertime;

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
