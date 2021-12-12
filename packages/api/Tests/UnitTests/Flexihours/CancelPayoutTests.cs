using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Overtime;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static Tests.UnitTests.Flexihours.GetOvertimeTests;

namespace Tests.UnitTests.Flexihours
{
    public class CancelPayoutTests
    {
        private readonly AlvTime_dbContext context;
        private readonly IOptionsMonitor<TimeEntryOptions> options;

        public CancelPayoutTests()
        {
            context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var entryOptions = new TimeEntryOptions
            {
                SickDaysTask = 14,
                PaidHolidayTask = 13,
                UnpaidHolidayTask = 19,
                FlexTask = 18,
                StartOfOvertimeSystem = new DateTime(2020, 01, 01),
                AbsenceProject = 9
            };
            options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);
        }

        [Fact]
        public void CancelPayout_PayoutIsRegisteredInSameMonth_PayoutIsCanceled()
        {
            var dbUser = context.User.First();
            dbUser.StartDate = new DateTime(2020, 11, 01);

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            context.Hours.Add(CreateTimeEntry(date: new DateTime(currentYear, currentMonth, 02), value: 17.5M, out int taskid));
            context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

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
            var dbUser = context.User.First();
            dbUser.StartDate = new DateTime(2020, 10, 01);

            var previousMonth = DateTime.Now.AddMonths(-1).Month;

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, previousMonth, 02), value: 17.5M, out int taskid));
            context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, previousMonth, 02),
                Hours = 5
            }, 1);

            Assert.Throws<ValidationException>(() => calculator.CancelPayout(1, 1));
        }

        private FlexhourStorage CreateFlexhourStorage()
        {
            return new FlexhourStorage(CreateTimeEntryStorage(), context, options);
        }
        
        public TimeEntryStorage CreateTimeEntryStorage()
        {
            return new TimeEntryStorage(context, CreateOvertimeService());
        }

        public OvertimeService CreateOvertimeService()
        {
            var mockUserContext = new Mock<IUserContext>();

            var user = new AlvTime.Business.Models.User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone"
            };

            mockUserContext.Setup(context => context.GetCurrentUser()).Returns(user);

            return new OvertimeService(new OvertimeStorage(context), mockUserContext.Object, new TaskStorage(context), options);
        }

        private static Hours CreateTimeEntry(DateTime date, decimal value, out int taskId)
        {
            taskId = new Random().Next();

            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                Task = new Task { Id = taskId, Project = 1 }
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
