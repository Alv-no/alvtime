using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Business.Services;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories;
using AlvTime.Persistence.Repositories.AlvEconomyData;
using Xunit;

namespace Tests.UnitTests.Flexihours
{
    public class RegisterPaidOvertimePayoutTests
    {
        private readonly AlvEconomyDataContext _economyDataContext =
            new AlvEconomyDataDbContextBuilder().CreateDbContext();

        private readonly AlvTime_dbContext _context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();
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

            var sut = CreateStorage();

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry { Date = new DateTime(2020, 01, 02), Hours = 1.0M });

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

            var sut = CreateStorage();

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 1.0M, Date = new DateTime(2021, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry { Date = new DateTime(2021, 08, 02), Hours = 2.0M });

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

            var sut = CreateStorage();

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry { Date = new DateTime(2020, 01, 02), Hours = 2.0M });

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

            var sut = CreateStorage();

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2021, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry { Date = new DateTime(2020, 01, 02), Hours = 2.0M });

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

            var sut = CreateStorage();

            var overtimeSalary = sut.RegisterOvertimePayout(
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



        private FlexhourStorage CreateStorage()
        {
            return new FlexhourStorage(new TimeEntryStorage(_context), _context, new GetOvertimeTests.TestTimeEntryOptions(
                    new TimeEntryOptions
                    {
                        FlexTask = 18,
                        ReportUser = 11,
                        StartOfOvertimeSystem = new DateTime(2020, 01, 01)
                    }),
                new SalaryService(new OvertimePayoutStorage(_economyDataContext), new EmployeeHourlySalaryStorage(_economyDataContext, _context)));
        }
    }
}