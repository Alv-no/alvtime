using System;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories.AlvEconomyData;
using Xunit;

namespace Tests.UnitTests.EconomyDataTests.Overtime
{
    public class EmployeeHourlySalaryStorageTests
    {
        [Fact]
        public void GetSalary_ReturnSalary()
        {
            var context = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(context);

            context.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 500.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = new DateTime(day: 01, month: 07, year: 2023)
            });
            context.SaveChanges();
            
            var salary = storage.GetHouerlySalary(1, new DateTime(day: 08, month: 08, year: 2021));
            
            Assert.Equal(500.0M, salary);
        }

        [Fact]
        public void GetHourlySalary_UserHasTwoSalaries_ReturnsFirst()
        {
            var context = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(context);

            context.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 200.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(day: 30, month: 06, year: 2020)
            });
            context.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = null
            });
            context.SaveChanges();

            var salary = storage.GetHouerlySalary(1, new DateTime(day: 08, month: 09, year: 2019));

            Assert.Equal(200.0M, salary);
        }

        [Fact]
        public void GetHourlySalary_UserHasTwoSalaries_ReturnsLast()
        {
            var context = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(context);

            context.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 200.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(day: 30, month: 06, year: 2020)
            });
            context.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = null
            });
            context.SaveChanges();

            var salary = storage.GetHouerlySalary(1, new DateTime(day: 08, month: 09, year: 2021));

            Assert.Equal(200.0M, salary);
        }
    }
}