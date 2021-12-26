using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.Payouts;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Utils;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests.UnitTests
{
    public class TimeRegistrationServiceTests
    {
        private readonly AlvTime_dbContext _context;
        private readonly IOptionsMonitor<TimeEntryOptions> _options;
        private readonly Mock<IUserContext> _userContextMock;
        
        public TimeRegistrationServiceTests()
        {
            _context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithLeaveTasks()
                .WithProjects()
                .WithUsers()
                .WithCustomers()
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
            _options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);

            _userContextMock = new Mock<IUserContext>();

            var user = new AlvTime.Business.Models.User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone"
            };

            _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user);
        }

        [Fact]
        public void UpsertTimeEntry_FlexBeforeARegisteredPayoutDate_ShouldNotBeAbleToFlex()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var payoutService = CreatePayoutService(timeRegistrationService);

            var timeEntry1 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 19.5M, 1.5M, out int taskId1); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            
            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 14),
                Hours = 1
            });

            var flexTimeEntry = CreateTimeEntryForExistingTask(new DateTime(2021, 12, 13), 1, 18);
            Assert.Throws<Exception>(() => timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId } }));
        }
        
        [Fact]
        public void UpsertTimeEntry_EarningOvertimeBeforeRegisteredPayoutDate_ShouldNotBeAbleToRegisterOvertime()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var payoutService = CreatePayoutService(timeRegistrationService);

            var timeEntry1 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 19.5M, 1.5M, out int taskId1); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            
            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 14),
                Hours = 1
            });

            var timeEntry2 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 08), 19.5M, 1.5M, out int taskId2); //Monday
            Assert.Throws<Exception>(() => timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } }));
        }
        
        private TimeRegistrationService CreateTimeRegistrationService()
        {
            return new TimeRegistrationService(_options, _userContextMock.Object, CreateTaskUtils(),
                new TimeRegistrationStorage(_context), new DbContextScope(_context), new PayoutStorage(_context), new CompensationRateStorage(_context));
        }

        private PayoutService CreatePayoutService(TimeRegistrationService timeRegistrationService)
        {
            return new PayoutService(new PayoutStorage(_context), _userContextMock.Object,
                timeRegistrationService);
        }

        private TaskUtils CreateTaskUtils()
        {
            return new TaskUtils(new TaskStorage(_context), _options);
        }
        
        private Hours CreateTimeEntryWithCompensationRate(DateTime date, decimal value, decimal compensationRate,
            out int taskId)
        {
            taskId = new Random().Next(1000, 10000000);
            var task = new Task {Id = taskId, Project = 1,};
            _context.Task.Add(task);
            _context.CompensationRate.Add(new CompensationRate
                {TaskId = taskId, Value = compensationRate, FromDate = new DateTime(2021, 01, 01)});
            _context.SaveChanges();

            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                Task = task,
                TaskId = taskId
            };
        }
        
        private static Hours CreateTimeEntryForExistingTask(DateTime date, decimal value, int taskId)
        {
            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                TaskId = taskId
            };
        }
    }
}