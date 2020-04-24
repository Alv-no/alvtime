using AlvTime.Business.TimeEntries;
using AlvTimeWebApi.Controllers.TimeEntries.TimeEntryStorage;
using System;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.TimeEntries
{
    public class TimeEntryStorageTests
    {
        [Fact]
        public void GetTimeEntries_DatesSpecified_AllEntriesBetweenDates()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new TimeEntryStorage(context);

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
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new TimeEntryStorage(context);

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
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new TimeEntryStorage(context);

            var previousAmountOfEntries = context.Hours.Count();

            storage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2020, 01, 01),
                TaskId = 2,
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
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new TimeEntryStorage(context);

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
    }
}
