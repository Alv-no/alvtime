using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.EconomyDataDBModels;
using Xunit;

namespace Tests.UnitTests.Flexihours
{
    public class RegisterOvertimePayoutTests
    {

        private readonly AlvTime_dbContext _context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();

        private readonly AlvEconomyDataContext _economyDataContext =
            new AlvEconomyDataDbContextBuilder().CreateDbContext();


        [Fact]
        public void RegisterOvertimePayoutSalary_1HourOvertimeWith1SalaryAndCompRate1_PayoutRegistered()
        {
            var userId = 1001;
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = null
            });

            _economyDataContext.SaveChanges();
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1}
                },
                userId,
                new GenericHourEntry { Date = new DateTime(2020, 01, 02), Hours = 1.0M },
                1);
            
            Assert.Equal(300.0M, overtimeSalary);
        }

        [Fact]
        public void RegisterOvertimePayoutSalary_2HoursOvertimeWith2DifferentSalariesSameCompRate_PayoutRegistered()
        {
            var userId = 1001;
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = new DateTime(2020, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 400.0M,
                FromDateInclusive = new DateTime(2020, 07, 01),
                ToDate = null
            });

            _economyDataContext.SaveChanges();
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 1.0M, Date = new DateTime(2021, 01, 01), Hours = 1, TaskId = 1}
                },
                userId,
                new GenericHourEntry { Date = new DateTime(2021, 08, 02), Hours = 2.0M },
                1);

            Assert.Equal(700.0M, overtimeSalary);
        }


        [Fact]
        public void RegisterOvertimePayoutSalary_2HoursOvertimeWith2DifferentCompensationRates1Salary__PayoutRegistered()
        {
            var userId = 1001;
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = null
            });

            _economyDataContext.SaveChanges();
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);
            
            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1}
                },
                userId,
                new GenericHourEntry { Date = new DateTime(2020, 01, 02), Hours = 2.0M },
                1);
            
            Assert.Equal(450.0M, overtimeSalary);
        }

        [Fact]
        public void
            RegisterOvertimePayoutSalary_2HoursOvertimeWith2DifferentSalariesAnd2DifferentCompensationRates_PayoutRegistered()
        {
            var userId = 1001;
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = new DateTime(2020, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 400.0M,
                FromDateInclusive = new DateTime(2020, 07, 01),
                ToDate = null
            });

            _economyDataContext.SaveChanges();
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2021, 01, 01), Hours = 1, TaskId = 1}
                },
                userId,
                new GenericHourEntry { Date = new DateTime(2020, 01, 02), Hours = 2.0M },
                1);

            Assert.Equal(500.0M, overtimeSalary);
        }

        [Fact]
        public void RegisterOvertimePayoutSalary_3hoursOvertime3SalariesOvertimeFrom1Salary_PayoutRegistered()
        {
            var userId = 1001;
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = new DateTime(2020, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(2020, 07, 01),
                ToDate = new DateTime(2021, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(2021, 07, 01),
                ToDate = null
            });

            _economyDataContext.SaveChanges();
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2020, 01, 02), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2020, 01, 03), Hours = 1, TaskId = 1}
                },
                userId,
                new GenericHourEntry { Date = new DateTime(2021, 07, 02), Hours = 3.0M },
                1);

            Assert.Equal(600.0M, overtimeSalary);
        }
    }
}
