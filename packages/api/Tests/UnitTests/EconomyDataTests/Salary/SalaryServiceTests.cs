using System;
using System.Globalization;
using AlvTime.Business.EconomyData;
using AlvTime.Business.Services;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories.AlvEconomyData;
using Xunit;
using System.Linq;
using FluentValidation;

namespace Tests.UnitTests.EconomyDataTests.Salary
{
    public class SalaryServiceTests
    {
        private readonly AlvEconomyDataContext _economyDataContext =
            new AlvEconomyDataDbContextBuilder().CreateDbContext();

        private readonly AlvTime_dbContext _context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();
        
        [Fact]
        public void RegisterHourlySalary_RegisterHourlySalaryForOneEmployee_SalaryIsRegistered()
        {
            var sut = CreateService();

            var employeeSalary = new EmployeeSalaryRequest(1, 120.0M, new DateTime(2020, 08, 12), null);

            sut.RegisterHourlySalary(employeeSalary);

            Assert.Single(_economyDataContext.EmployeeHourlySalaries.Where(e => e.UserId == 1));
            Assert.Equal(employeeSalary.HourlySalary, _economyDataContext.EmployeeHourlySalaries.FirstOrDefault(e => e.UserId == 1).HourlySalary);
            Assert.Equal(employeeSalary.FromDate, _economyDataContext.EmployeeHourlySalaries.FirstOrDefault(e => e.UserId == 1).FromDateInclusive);
            Assert.Null(_economyDataContext.EmployeeHourlySalaries.FirstOrDefault(e => e.UserId == 1).ToDate);
        }

        [Fact]
        public void RegisterHourlySalary_EmployeeIsNotRegistered_ThrowsException()
        {
            var economyContext = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var context = new AlvTimeDbContextBuilder().CreateDbContext();
            var sut = new SalaryService(new OvertimePayoutStorage(economyContext), new EmployeeHourlySalaryStorage(economyContext, context));

            var employeeSalary = new EmployeeSalaryRequest(1, 120.0M, new DateTime(2020, 08, 12), null);

            Assert.Throws<ValidationException>(() => sut.RegisterHourlySalary(employeeSalary));
        }

        [Fact]
        public void RegisterHourlySalary_UpdatesHourlySalaryForOneEmployee_SalaryIsRegistered()
        {
            var sut = CreateService();

            var oldSalary = new EmployeeHourlySalary
            {
                UserId = 1,
                FromDateInclusive = new DateTime(2020, 01, 01),
                HourlySalary = 100.0M,
                ToDate = null
            };

            _economyDataContext.EmployeeHourlySalaries.Add(oldSalary);
            _economyDataContext.SaveChanges();

            var newSalary = new EmployeeSalaryRequest(1,  120.0M, new DateTime(2020, 08, 12), null);

            sut.RegisterHourlySalary(newSalary);

            Assert.Equal(2, _economyDataContext.EmployeeHourlySalaries.Count(e => e.UserId == 1));

            //verifies that old salary is updated with new ToDate
            Assert.Single(_economyDataContext.EmployeeHourlySalaries.Where(x => x.UserId == oldSalary.UserId &&
                                                                          x.FromDateInclusive == oldSalary.FromDateInclusive &&
                                                                          x.HourlySalary == oldSalary.HourlySalary &&
                                                                          x.ToDate == newSalary.FromDate.AddDays(-1)));
            //verifies that new salary is added with correct values
            Assert.Single(_economyDataContext.EmployeeHourlySalaries.Where(x => x.UserId == newSalary.UserId &&
                                                                           x.FromDateInclusive == newSalary.FromDate &&
                                                                           x.HourlySalary == newSalary.HourlySalary &&
                                                                           x.ToDate == null));

        }

        [Fact]
        public void GetEmployeeSalaryData_EmployeeHasSalary_ReturnsSalary()
        {
            var sut = CreateService();

            var firstSalary =
                new EmployeeHourlySalary
                {
                    UserId = 1,
                    HourlySalary = 200.0M,
                    FromDateInclusive = new DateTime(2019, 07, 01),
                    ToDate = new DateTime(2020, 06, 30)
                };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(2020, 07, 01),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);

            _economyDataContext.SaveChanges();

            var salaryData = sut.GetEmployeeSalaryData(1);

            Assert.Equal(2, salaryData.Count);
            Assert.Equal(firstSalary.HourlySalary + secondSalary.HourlySalary, salaryData.Sum(x => x.HourlySalary));
        }

        [Fact]
        public void GetEmployeeSalaryData_MoreThanOneEmployee_ReturnsSalaryForOneEmployee()
        {
            var sut = CreateService();

            var salaryFirstEmployee =
                new EmployeeHourlySalary
                {
                    UserId = 1,
                    HourlySalary = 200.0M,
                    FromDateInclusive = new DateTime(2019, 07, 01),
                    ToDate = new DateTime(2020, 06, 30)
                };
            var salarySecondEmployee = new EmployeeHourlySalary
            {
                UserId = 2,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(2020, 07, 01),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(salaryFirstEmployee);
            _economyDataContext.EmployeeHourlySalaries.Add(salarySecondEmployee);

            _economyDataContext.SaveChanges();

            var salaryData = sut.GetEmployeeSalaryData(1);

            Assert.Single(salaryData);
            Assert.Equal(salaryFirstEmployee.HourlySalary, salaryData[0].HourlySalary);
        }

        [Fact]
        public void GetEmployeeSalaryData_EmployeeDoesNotExist_ReturnsError()
        {
            var economyDataContext = new AlvEconomyDataDbContextBuilder().CreateDbContext();
            var context = new AlvTimeDbContextBuilder().CreateDbContext();
            var storage = new SalaryService(new OvertimePayoutStorage(economyDataContext),
                new EmployeeHourlySalaryStorage(economyDataContext, context));

            Assert.Throws<ValidationException>(() => storage.GetEmployeeSalaryData(1001));
        }

        [Fact]
        public void DeleteOvertimePayout_TotalPayoutIsDeleted()
        {
            var sut = CreateService();

            var overTimePayoutForDeletion = new OvertimePayout
            {
                Date = new DateTime(day: 11, month: 08, year: 2021),
                UserId = 1,
                TotalPayout = 10.0M,
                RegisteredPaidOvertimeId = 1
            };

            _economyDataContext.OvertimePayouts.Add(overTimePayoutForDeletion);
            _economyDataContext.SaveChanges();

            var deletedOvertimePayout =
                sut.DeleteOvertimePayout(overTimePayoutForDeletion.UserId, overTimePayoutForDeletion.RegisteredPaidOvertimeId);

            Assert.Equal(overTimePayoutForDeletion.UserId, deletedOvertimePayout.UserId);
            Assert.Equal(overTimePayoutForDeletion.Date, deletedOvertimePayout.Date);
            Assert.Equal(overTimePayoutForDeletion.TotalPayout, deletedOvertimePayout.TotalPayout);
            Assert.Equal(1, deletedOvertimePayout.Id);
        }

        private ISalaryService CreateService()
        {
            return new SalaryService(new OvertimePayoutStorage(_economyDataContext), new EmployeeHourlySalaryStorage(
            _economyDataContext, _context
        ));
        }
    }
}
