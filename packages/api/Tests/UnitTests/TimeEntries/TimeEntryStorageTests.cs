using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using AlvTime.Business.Options;
using AlvTime.Business.TimeRegistration;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests.UnitTests.TimeEntries;

public class TimeEntryStorageTests
{
    private readonly AlvTime_dbContext _context;

    public TimeEntryStorageTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .WithCustomers()
            .WithProjects()
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
        Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTimeEntries_DatesSpecified_AllEntriesBetweenDates()
    {
        var storage = CreateTimeEntryStorage();

        var timeEntries = await storage.GetTimeEntries(new TimeEntryQuerySearch
        {
            UserId = 1,
            FromDateInclusive = new DateTime(2019, 01, 01),
            ToDateInclusive = new DateTime(2020, 01, 01)
        });

        var contextCountInPeriod = _context.Hours
            .Where(x => x.Date.Date <= new DateTime(2020, 01, 01) && x.Date.Date >= new DateTime(2019, 01, 01) && x.User == 1)
            .ToList();

        Assert.Equal(contextCountInPeriod.Count(), timeEntries.Count());
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTimeEntries_TaskSpecified_AllEntriesWithSpecifiedTask()
    {
        var storage = CreateTimeEntryStorage();

        var timeEntries = await storage.GetTimeEntries(new TimeEntryQuerySearch
        {
            UserId = 1,
            TaskId = 2
        });

        var contextEntriesWithTask = _context.Hours
            .Where(x => x.TaskId == 2 && x.User == 1)
            .ToList();

        Assert.Equal(contextEntriesWithTask.Count, timeEntries.Count());
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTimeEntry_NewTimeEntry_TimeEntryCreated()
    {
        var storage = CreateTimeEntryStorage();

        var previousAmountOfEntries = _context.Hours.Count();

        await storage.CreateTimeEntry(new CreateTimeEntryDto
        {
            Date = new DateTime(2020, 01, 01),
            TaskId = 1,
            Value = 7.5M
        }, 1);

        var timeEntries = await storage.GetTimeEntries(new TimeEntryQuerySearch
        {
            UserId = 1,
            FromDateInclusive = new DateTime(2010, 01, 01),
            ToDateInclusive = new DateTime(2030, 01, 01)
        });

        Assert.Equal(previousAmountOfEntries + 1, timeEntries.Count());
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTimeEntry_ExistingTimeEntry_TimeEntryUpdated()
    {
        var storage = CreateTimeEntryStorage();

        await storage.UpdateTimeEntry(new CreateTimeEntryDto
        {
            Date = new DateTime(2020, 05, 02),
            TaskId = 1,
            Value = 10
        }, 1);

        var timeEntry = await storage.GetTimeEntry(new TimeEntryQuerySearch
        {
            UserId = 1,
            FromDateInclusive = new DateTime(2020, 05, 02),
            ToDateInclusive = new DateTime(2020, 05, 02),
            TaskId = 1
        });

        Assert.True(timeEntry.Value == 10);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTimeEntriesWithCustomer_BetweenDates_ForSingularUser()
    {

        var storage = CreateTimeEntryStorage();
        var stats = await storage.GetTimeEntriesWithCustomer(1, new DateTime(2019, 05, 02), new DateTime(2019, 05, 02).AddHours(20));

        // In that date-range we have exactly two entries
        Assert.Equal(2, stats.Count());

        var taskWithCustomer = stats.Find(task => task.TaskId == 1);

        Assert.Equal("ExampleCustomer", taskWithCustomer.CustomerName);
        Assert.Equal(6, taskWithCustomer.Value);
    }

    private TimeRegistrationStorage CreateTimeEntryStorage()
    {
        return new TimeRegistrationStorage(_context);
    }


}
