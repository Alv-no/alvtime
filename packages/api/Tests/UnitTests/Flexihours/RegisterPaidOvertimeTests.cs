﻿using AlvTime.Business.FlexiHours;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
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

            Assert.Equal(10, registerOvertimeResponse.HoursBeforeCompRate);
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
            }, 1).Value as PaidOvertime;

            Assert.Equal(5, registeredPayout.HoursAfterCompRate);
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
            }, 1).Value as PaidOvertime;

            Assert.Equal(3.5M, registeredPayout.HoursAfterCompRate);
            Assert.Equal(6M, registeredPayout.HoursBeforeCompRate);
        }

        [Fact]
        public void RegisterPayout_NotEnoughOvertime_CannotRegisterPayout()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 11.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var result = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 7
            }, 1).Value;

            Assert.Equal("Not enough available hours", result);
        }

        [Fact]
        public void RegisterPayout_RegisteringPayoutBeforeWorkingOvertime_NoPayout()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 11.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var result = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 1
            }, 1).Value;

            Assert.Equal("Not enough available hours", result);
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