using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using AlvTime.Business.Services;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories.AlvEconomyData;
using FluentValidation;
using Xunit;
using static Tests.UnitTests.Flexihours.GetOvertimeTests;

namespace Tests.UnitTests.Flexihours
{
    public class CancelPayoutTests
    {
        private AlvTime_dbContext _context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();
        private AlvEconomyDataContext _economyDataContext = new AlvEconomyDataDbContextBuilder().WithEmployeeSalary().CreateDbContext();

        [Fact]
        public void CancelPayout_PayoutIsRegisteredInSameMonth_PayoutIsCanceled()
        {
            var dbUser = _context.User.First();
            dbUser.StartDate = new DateTime(2020, 11, 01);

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(currentYear, currentMonth, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();
            _economyDataContext.OvertimePayouts.Add(new OvertimePayout{Date = new DateTime(currentYear, currentMonth, 02) , TotalPayout = 150M, UserId = dbUser.Id, RegisteredPaidOvertimeId = 1});

            _economyDataContext.SaveChanges();

            FlexhourStorage calculator = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(currentYear, currentMonth, 02),
                Hours = 10
            }, 1);

            var canceledPayout = calculator.CancelPayout(1, 1);

            Assert.Equal(1, canceledPayout.Id);
        }

        [Fact]
        public void CancelPayout_PayoutIsRegisteredPreviousMonth_PayoutIsLocked()
        {
            var dbUser = _context.User.First();
            dbUser.StartDate = new DateTime(2020, 10, 01);

            var previousMonth = DateTime.Now.AddMonths(-1).Month;

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, previousMonth, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, previousMonth, 02),
                Hours = 5
            }, 1);

            Assert.Throws<ValidationException>(() => calculator.CancelPayout(1, 1));
        }
        
    }
}
