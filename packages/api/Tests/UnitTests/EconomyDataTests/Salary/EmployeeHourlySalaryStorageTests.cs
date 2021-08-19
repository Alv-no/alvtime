using System;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories.AlvEconomyData;
using Xunit;

namespace Tests.UnitTests.EconomyDataTests.Salary
{
    public class EmployeeHourlySalaryStorageTests
    {
        [Fact]
        public void GetHourlySalary_ReturnSalary()
        {
            var economyContext = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var context = new AlvTimeDbContextBuilder().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(economyContext, context);

            economyContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 500.0M,
                FromDateInclusive = new DateTime(2020,07,01),
                ToDate = new DateTime( 2023,07,01)
            });
            economyContext.SaveChanges();

            var salary = storage.GetHourlySalary(1, new DateTime(2021,08,08));

            Assert.Equal(500.0M, salary);
        }

        [Fact]
        public void GetHourlySalary_UserHasTwoSalaries_ReturnsFirst()
        {
            var economyContext = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var context = new AlvTimeDbContextBuilder().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(economyContext, context);

            economyContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 200.0M,
                FromDateInclusive = new DateTime(2019,07,01),
                ToDate = new DateTime(2020,06,30)
            });
            economyContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(202,07,01),
                ToDate = null
            });
            economyContext.SaveChanges();

            var salary = storage.GetHourlySalary(1, new DateTime(2019,09,08));

            Assert.Equal(200.0M, salary);
        }

        [Fact]
        public void GetHourlySalary_UserHasTwoSalaries_ReturnsLast()
        {
            var economyContext = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var context = new AlvTimeDbContextBuilder().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(economyContext, context);

            var firstSalary =
                new EmployeeHourlySalary
                {
                    UserId = 1,
                    HourlySalary = 200.0M,
                    FromDateInclusive = new DateTime(2019,07,01),
                    ToDate = new DateTime(2020,06,30)
                };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(2020,07,01),
                ToDate = null
            };
            economyContext.EmployeeHourlySalaries.Add(firstSalary);
            economyContext.EmployeeHourlySalaries.Add(secondSalary);

            economyContext.SaveChanges();

            var salary = storage.GetHourlySalary(1, new DateTime(2021,09,08));

            Assert.Equal(secondSalary.HourlySalary, salary);
        }

    }
}