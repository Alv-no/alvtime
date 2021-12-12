using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.Overtime;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests.UnitTests.TimeEntries
{
    public class TimeEntryStorageTests
    {
        private readonly AlvTime_dbContext context;
        private readonly IOptionsMonitor<TimeEntryOptions> options;

        public TimeEntryStorageTests()
        {
            context = new AlvTimeDbContextBuilder()
                .WithTimeEntries()
                .WithTasks()
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
        public void GetTimeEntries_DatesSpecified_AllEntriesBetweenDates()
        {
            var storage = CreateTimeEntryStorage();

            var timeEntries = storage.GetTimeEntries(new TimeEntryQuerySearch
            {
                UserId = 1,
                FromDateInclusive = new DateTime(2019, 01, 01),
                ToDateInclusive = new DateTime(2020, 01, 01)
            });

            var contextCountInPeriod = context.Hours
                .Where(x => x.Date.Date <= new DateTime(2020, 01, 01) && x.Date.Date >= new DateTime(2019, 01, 01) && x.User == 1)
                .ToList();

            Assert.Equal(contextCountInPeriod.Count(), timeEntries.Count());
        }

        [Fact]
        public void GetTimeEntries_TaskSpecified_AllEntriesWithSpecifiedTask()
        {
            var storage = CreateTimeEntryStorage();

            var timeEntries = storage.GetTimeEntries(new TimeEntryQuerySearch
            {
                UserId = 1,
                TaskId = 2
            });

            var contextEntriesWithTask = context.Hours
                .Where(x => x.TaskId == 2 && x.User == 1)
                .ToList();

            Assert.Equal(contextEntriesWithTask.Count(), timeEntries.Count());
        }

        [Fact]
        public void CreateTimeEntry_NewTimeEntry_TimeEntryCreated()
        {
            var storage = CreateTimeEntryStorage();

            var previousAmountOfEntries = context.Hours.Count();

            storage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2020, 01, 01),
                TaskId = 1,
                Value = 7.5M
            }, 1);

            var timeEntries = storage.GetTimeEntries(new TimeEntryQuerySearch
            {
                UserId = 1,
                FromDateInclusive = new DateTime(2010, 01, 01),
                ToDateInclusive = new DateTime(2030, 01, 01)
            });

            Assert.Equal(previousAmountOfEntries + 1, timeEntries.Count());
        }

        [Fact]
        public void UpdateTimeEntry_ExistingTimeEntry_TimeEntryUpdated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTimeEntries()
                .WithTasks()
                .WithUsers()
                .CreateDbContext();

            var storage = CreateTimeEntryStorage();

            storage.UpdateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2020, 05, 02),
                TaskId = 1,
                Value = 10
            }, 1);

            var timeEntry = storage.GetTimeEntry(new TimeEntryQuerySearch
            {
                UserId = 1,
                FromDateInclusive = new DateTime(2020, 05, 02),
                ToDateInclusive = new DateTime(2020, 05, 02),
                TaskId = 1
            });

            Assert.True(timeEntry.Value == 10);
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
    }
}
