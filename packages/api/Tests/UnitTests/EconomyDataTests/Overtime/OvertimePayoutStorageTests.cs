using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.EconomyData;
using AlvTime.Business.FlexiHours;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories.AlvEconomyData;
using Xunit;

namespace Tests.UnitTests.EconomyDataTests.Overtime
{
    public class OvertimePayoutStorageTests
    {
        private readonly AlvEconomyDataContext _economyDataContext =
            new AlvEconomyDataDbContextBuilder().CreateDbContext();

        private readonly AlvTime_dbContext _context = new AlvTimeDbContextBuilder().CreateDbContext();

        [Fact]
        public void SaveTotalOvertimePayout_TotalPayoutIsAdded()
        {
            var storage = CreateStorage();
            var overtimePayout = new OvertimePayout
            {
                UserId = 1,
                Date = new DateTime(day: 11, month: 08, year: 2021),
                TotalPayout = 1200.5M
            };

            storage.SaveTotalOvertimePayout(new RegisterOvertimePayoutDto
            {
                Date = overtimePayout.Date,
                UserId = overtimePayout.UserId,
                TotalPayout = overtimePayout.TotalPayout
            });

            Assert.Equal(1, _economyDataContext.OvertimePayouts.FirstOrDefault(op => op.Id == 1).Id);
            Assert.Equal(overtimePayout.UserId,
                _economyDataContext.OvertimePayouts.FirstOrDefault(op => op.Id == 1).UserId);
            Assert.Equal(overtimePayout.Date,
                _economyDataContext.OvertimePayouts.FirstOrDefault(op => op.Id == 1).Date);
            Assert.Equal(overtimePayout.TotalPayout,
                _economyDataContext.OvertimePayouts.FirstOrDefault(op => op.Id == 1).TotalPayout);
        }

        [Fact]
        public void DeleteOvertimePayout_TotalPayoutIsDeleted()
        {
            var storage = CreateStorage();

            var overTimePayoutForDeletion = new OvertimePayout
            {
                Date = new DateTime(day: 11, month: 08, year: 2021),
                UserId = 1, TotalPayout = 10.0M
            };

            _economyDataContext.OvertimePayouts.Add(overTimePayoutForDeletion);
            _economyDataContext.SaveChanges();

            var deletedOvertimePayout =
                storage.DeleteOvertimePayout(overTimePayoutForDeletion.UserId, overTimePayoutForDeletion.Date);

            Assert.Equal(overTimePayoutForDeletion.UserId, deletedOvertimePayout.UserId);
            Assert.Equal(overTimePayoutForDeletion.Date, deletedOvertimePayout.Date);
            Assert.Equal(overTimePayoutForDeletion.TotalPayout, deletedOvertimePayout.TotalPayout);
            Assert.Equal(1, deletedOvertimePayout.Id);
        }

        [Fact]
        public void RegisterOvertimePayoutSalary_1HourOvertimeWith1SalaryAndCompRate1_SalaryRegistered()
        {
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = null
            });

            _economyDataContext.SaveChanges();

            var payoutStorage = CreateStorage();

            var overtimeSalary = payoutStorage.RegisterOvertimePayoutSalary(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry {Date = new DateTime(2020, 01, 02), Hours = 1.0M});

            Assert.Equal(300.0M, overtimeSalary);
        }


        [Fact]
        public void RegisterOvertimePayoutSalary_2HoursOvertimeWith2DifferentSalariesSameCompRate_SalaryRegistered()
        {
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 400.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = null
            });

            _economyDataContext.SaveChanges();

            var overtimePayoutStorage = CreateStorage();

            var overtimeSalary = overtimePayoutStorage.RegisterOvertimePayoutSalary(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 1.0M, Date = new DateTime(2021, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry {Date = new DateTime(2021, 08, 02), Hours = 2.0M});

            Assert.Equal(700.0M, overtimeSalary);
        }


        [Fact]
        public void RegisterOvertimePayoutSalary_2HoursOvertimeWith2DifferentCompensationRates1Salary__SalaryRegistered()
        {
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = null
            });

            _economyDataContext.SaveChanges();

            var overtimePayoutStorage = CreateStorage();

            var overtimeSalary = overtimePayoutStorage.RegisterOvertimePayoutSalary(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry {Date = new DateTime(2020, 01, 02), Hours = 2.0M});

            Assert.Equal(450.0M, overtimeSalary);
        }

        [Fact]
        public void
            RegisterOvertimePayoutSalary_2HoursOvertimeWith2DifferentSalariesAnd2DifferentCompensationRates__SalaryRegistered()
        {
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 400.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = null
            });

            _economyDataContext.SaveChanges();

            var overtimePayoutStorage = CreateStorage();

            var overtimeSalary = overtimePayoutStorage.RegisterOvertimePayoutSalary(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2021, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry {Date = new DateTime(2020, 01, 02), Hours = 2.0M});

            Assert.Equal(500.0M, overtimeSalary);
        }

        [Fact]
        public void RegisterOvertimePayoutSalary_3hoursOvertime3SalariesOvertimeFrom1Salary_SalaryRegistered()
        {
            
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = new DateTime(2021, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2021),
                ToDate = null
            });

            _economyDataContext.SaveChanges();

            var overtimePayoutStorage = CreateStorage();

            var overtimeSalary = overtimePayoutStorage.RegisterOvertimePayoutSalary(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2020, 01, 02), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2020, 01, 03), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry { Date = new DateTime(2021, 07, 02), Hours = 3.0M });

            Assert.Equal(600.0M, overtimeSalary);
        }

        private OvertimePayoutStorage CreateStorage()
        {
            return new(_economyDataContext, new EmployeeHourlySalaryStorage(_economyDataContext, _context));
        }
    }
}