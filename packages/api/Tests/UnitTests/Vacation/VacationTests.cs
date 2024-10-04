using AlvTime.Business.Options;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using AlvTime.Business.Utils;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.Vacation;

public class VacationTests
{
    private readonly AlvTime_dbContext _context;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly TimeRegistrationService _timeRegistrationService;

    public VacationTests()
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
            PaidHolidayTask = 13,
            StartOfOvertimeSystem = new DateTime(2020, 01, 01)
        };

        _options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);
        _userContextMock = new Mock<IUserContext>();

        var user = new AlvTime.Business.Users.User
        {
            Id = 1,
            Email = "someone@alv.no",
            Name = "Someone"
        };

        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(System.Threading.Tasks.Task.FromResult(user));

        _timeRegistrationService = CreateTimeRegistrationService();
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterVacation_CorrectHoursAmountFullTimeWorker_VacationRegistered()
    {
        var dateToTest = new DateTime(2021, 12, 13);
        var hours = 7.5M;
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = dateToTest, Value = hours, TaskId = _options.CurrentValue.PaidHolidayTask}});

        var vacationEntries = await _timeRegistrationService.GetTimeEntries(new TimeEntryQuerySearch
        {
            TaskId = _options.CurrentValue.PaidHolidayTask
        });

        Assert.Single(vacationEntries);

        var vacationEntry = vacationEntries.First();

        Assert.Equal(hours, vacationEntry.Value);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterVacation_ZeroHours_VacationRegistered()
    {
        var dateToTest = new DateTime(2021, 12, 13);
        var hours = 0;
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = dateToTest, Value = hours, TaskId = _options.CurrentValue.PaidHolidayTask}});

        var vacationEntries = await _timeRegistrationService.GetTimeEntries(new TimeEntryQuerySearch
        {
            TaskId = _options.CurrentValue.PaidHolidayTask
        });

        Assert.Single(vacationEntries);

        var vacationEntry = vacationEntries.First();

        Assert.Equal(hours, vacationEntry.Value);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterVacation_TooFewHours_VacationRegisteredFailed()
    {
        var dateToTest = new DateTime(2021, 12, 13);
        var hours = 6M;
        var timeEntryResult = await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = dateToTest, Value = hours, TaskId = _options.CurrentValue.PaidHolidayTask}});

        Assert.False(timeEntryResult.IsSuccess);
        Assert.True(timeEntryResult.Errors.Any());
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterVacation_TooManyHours_VacationRegisteredFailed()
    {
        var dateToTest = new DateTime(2021, 12, 13);
        var hours = 9M;
        var timeEntryResult = await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = dateToTest, Value = hours, TaskId = _options.CurrentValue.PaidHolidayTask}});

        Assert.False(timeEntryResult.IsSuccess);
        Assert.True(timeEntryResult.Errors.Any());
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterVacation_CorrectHoursAmountPartTimeWorker_VacationRegistered()
    {
        _context.EmploymentRate.Add(new EmploymentRate
        {
            UserId = 1,
            Rate = 0.5M,
            FromDate = new DateTime(2021, 01, 01),
            ToDate = new DateTime(2022, 01, 08)
        });

        await _context.SaveChangesAsync();

        var dateToTest = new DateTime(2021, 12, 13);
        var hours = 3.75M;
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = dateToTest, Value = hours, TaskId = _options.CurrentValue.PaidHolidayTask}});

        var vacationEntries = await _timeRegistrationService.GetTimeEntries(new TimeEntryQuerySearch
        {
            TaskId = _options.CurrentValue.PaidHolidayTask
        });

        Assert.Single(vacationEntries);

        var vacationEntry = vacationEntries.First();

        Assert.Equal(hours, vacationEntry.Value);
    }

    private TimeRegistrationService CreateTimeRegistrationService()
    {
        return new TimeRegistrationService(_options, _userContextMock.Object, CreateTaskUtils(),
            new TimeRegistrationStorage(_context), new DbContextScope(_context), new PayoutStorage(_context, new DateAlvTime()), new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)));
    }

    private TaskUtils CreateTaskUtils()
    {
        return new TaskUtils(new TaskStorage(_context), _options);
    }
}