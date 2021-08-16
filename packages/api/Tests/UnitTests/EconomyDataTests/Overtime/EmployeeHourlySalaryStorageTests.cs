using System;
using System.Linq;
using AlvTime.Business.EconomyData;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories.AlvEconomyData;
using FluentValidation;
using Xunit;

namespace Tests.UnitTests.EconomyDataTests.Overtime
{
    public class EmployeeHourlySalaryStorageTests
    {
        [Fact]
        public void GetSalary_ReturnSalary()
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

            var salary = storage.GetHouerlySalary(1, new DateTime(2021,08,08));

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

            var salary = storage.GetHouerlySalary(1, new DateTime(2019,09,08));

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

            var salary = storage.GetHouerlySalary(1, new DateTime(2021,09,08));

            Assert.Equal(secondSalary.HourlySalary, salary);
        }

        [Fact]
        public void GetEmployeeSalaryData_EmployeeHasSalary_ReturnsSalary()
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

            var salaryData = storage.GetEmployeeSalaryData(1);

            Assert.Equal(2, salaryData.Count);
            Assert.Equal(firstSalary.HourlySalary + secondSalary.HourlySalary, salaryData.Sum(x => x.HourlySalary));
        }

        [Fact]
        public void GetEmployeeSalaryData_MoreThanOneEmployee_ReturnsSalaryForOneEmployee()
        {
            var economyContext = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var context = new AlvTimeDbContextBuilder().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(economyContext, context);

            var salaryFirstEmployee =
                new EmployeeHourlySalary
                {
                    UserId = 1,
                    HourlySalary = 200.0M,
                    FromDateInclusive = new DateTime( 2019, 07,01),
                    ToDate = new DateTime(2020,06,30)
                };
            var salarySecondEmployee = new EmployeeHourlySalary
            {
                UserId = 2,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(2020,07,01),
                ToDate = null
            };
            economyContext.EmployeeHourlySalaries.Add(salaryFirstEmployee);
            economyContext.EmployeeHourlySalaries.Add(salarySecondEmployee);

            economyContext.SaveChanges();

            var salaryData = storage.GetEmployeeSalaryData(1);

            Assert.Single(salaryData);
            Assert.Equal(salaryFirstEmployee.HourlySalary, salaryData[0].HourlySalary);
        }

        [Fact]
        public void GetEmployeeSalaryData_EmployeeDoesNotExist_ReturnsError()
        {
            var economyContext = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var context = new AlvTimeDbContextBuilder().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(economyContext, context);

            Assert.Throws<ValidationException>(() => storage.GetEmployeeSalaryData(1001));
        }


        [Fact]
        public void RegisterHourlySalary_RegisterHourlySalaryForOneEmployee_SalaryIsRegistered()
        {
            var economyContext = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(economyContext, context);

            var employeeSalary = new EmployeeSalaryDto
                {FromDate = new DateTime(2020, 08, 12), HourlySalary = 120.0M, usiderId = 1};
            
            storage.RegisterHourlySalary(employeeSalary);
            
            Assert.Single( economyContext.EmployeeHourlySalaries.Where(e => e.UserId == 1));
            Assert.Equal(employeeSalary.HourlySalary, economyContext.EmployeeHourlySalaries.FirstOrDefault(e => e.UserId == 1).HourlySalary);
            Assert.Equal(employeeSalary.FromDate, economyContext.EmployeeHourlySalaries.FirstOrDefault(e => e.UserId == 1).FromDateInclusive); 
            Assert.Null( economyContext.EmployeeHourlySalaries.FirstOrDefault(e => e.UserId == 1).ToDate);
        }

        [Fact]
        public void RegisterHourlySalary_EmployeeIsNotRegistered_ThrowsException()
        {
            var economyContext = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var context = new AlvTimeDbContextBuilder().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(economyContext, context);

            var employeeSalary = new EmployeeSalaryDto
                { FromDate = new DateTime(2020, 08, 12), HourlySalary = 120.0M, usiderId = 1 };

            Assert.Throws<ValidationException>(() => storage.RegisterHourlySalary(employeeSalary));
        }

        [Fact]
        public void RegisterHourlySalary_UpdatesHourlySalaryForOneEmployee_SalaryIsRegistered()
        {
            var economyContext = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();
            var storage = new EmployeeHourlySalaryStorage(economyContext, context);
            
            var oldSalary = new EmployeeHourlySalary
            {
                UserId = 1,
                FromDateInclusive = new DateTime(2020, 01, 01),
                HourlySalary = 100.0M,
                ToDate = null
            };

            economyContext.EmployeeHourlySalaries.Add(oldSalary);
            economyContext.SaveChanges();

            var newSalary = new EmployeeSalaryDto
                { FromDate = new DateTime(2020, 08, 12), HourlySalary = 120.0M, usiderId = 1 };

            storage.RegisterHourlySalary(newSalary);

            Assert.Equal(2,economyContext.EmployeeHourlySalaries.Count(e => e.UserId == 1));
            
            //varifies that old salary is updated with new ToDate
            Assert.Single(economyContext.EmployeeHourlySalaries.Where(x => x.UserId == oldSalary.UserId &&
                                                                          x.FromDateInclusive == oldSalary.FromDateInclusive &&
                                                                          x.HourlySalary== oldSalary.HourlySalary && 
                                                                          x.ToDate == newSalary.FromDate.AddDays(-1)));
            //varifies that new salary is added with correct values
            Assert.Single(economyContext.EmployeeHourlySalaries.Where(x => x.UserId == newSalary.usiderId &&
                                                                           x.FromDateInclusive == newSalary.FromDate &&
                                                                           x.HourlySalary == newSalary.HourlySalary &&
                                                                           x.ToDate == null));
            
        }
    }
}