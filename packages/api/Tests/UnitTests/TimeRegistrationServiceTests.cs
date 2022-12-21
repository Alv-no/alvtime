using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.Payouts;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using AlvTime.Business.Utils;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Tests.UnitTests.Utils;
using Xunit;

namespace Tests.UnitTests;

public class TimeRegistrationServiceTests
{
    private readonly AlvTime_dbContext _context;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly TimeRegistrationService _timeRegistrationService;
    private readonly PayoutService _payoutService;

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

        _timeRegistrationService = CreateTimeRegistrationService();

        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(System.Threading.Tasks.Task.FromResult(user));

        var payoutValidationServiceMock = new Mock<PayoutValidationService>(new UserService(new UserRepository(_context)),
            _timeRegistrationService, new PayoutStorage(_context));
        payoutValidationServiceMock.Setup(x => x.CheckForIncompleteDays(It.IsAny<GenericHourEntry>(), It.IsAny<int>())).Returns(System.Threading.Tasks.Task.FromResult(System.Threading.Tasks.Task.CompletedTask));
        payoutValidationServiceMock.CallBase = true;
        _payoutService = new PayoutService(new PayoutStorage(_context), _userContextMock.Object,
            _timeRegistrationService, payoutValidationServiceMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertTimeEntry_FlexBeforeARegisteredPayoutDate_ShouldNotBeAbleToFlex()
    {

        var timeEntry1 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 19.5M, 1.5M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

        await _payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2021, 12, 14),
            Hours = 1
        });

        var flexTimeEntry = CreateTimeEntryForExistingTask(new DateTime(2021, 12, 13), 1, 18);
        await Assert.ThrowsAsync<Exception>(async () => await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId } }));
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertTimeEntry_EarningOvertimeBeforeRegisteredPayoutDate_ShouldNotBeAbleToRegisterOvertime()
    {
        var timeEntry1 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 19.5M, 1.5M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

        await _payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2021, 12, 14),
            Hours = 1
        });

        var timeEntry2 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 08), 19.5M, 1.5M, out _); //Monday
        await Assert.ThrowsAsync<Exception>(async () => await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } }));
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertTimeEntry_RecordedVacationOnChristmas_CannotRegisterVactionOnRedDay()
    {
        var vacationEntry =
            CreateTimeEntryForExistingTask(new DateTime(2021, 12, 24), 7.5M, 13);

        await Assert.ThrowsAsync<Exception>(async () => await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = vacationEntry.Date, Value = vacationEntry.Value, TaskId = vacationEntry.TaskId}}));
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterTimeEntry_PayoutWouldBeAffected_ExceptionThrown()
    {
        var timeEntry1 =
            CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 28), 5M, 0.5M, out _); //Friday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var timeEntry2 =
            CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 28), 5M, 1.5M, out int taskId2); //Friday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });

        await _payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2022, 01, 28), //Friday
            Hours = 2.5M
        });

        var timeEntry3 =
            CreateTimeEntryForExistingTask(new DateTime(2022, 01, 28), 2M, taskId2); //Friday
        await Assert.ThrowsAsync<Exception>(async () => await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId } }));
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterFlex_HasFutureFlex_ExceptionThrown()
    {
        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;
        var currentDay = DateTime.Now.Day;
        var overtimeEntry =
            CreateTimeEntryWithCompensationRate(new DateTime(currentYear, currentMonth, currentDay), 12M, 1.5M, out _);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = overtimeEntry.Date, Value = overtimeEntry.Value, TaskId = overtimeEntry.TaskId } });

        var futureDayToRegisterFlexOn = DateUtils.GetFutureNonWeekendDay();
        var futureFlex =
            CreateTimeEntryForExistingTask(futureDayToRegisterFlexOn, 1M, 18);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = futureFlex.Date, Value = futureFlex.Value, TaskId = futureFlex.TaskId } });

        var dayToRegisterFlexOn = DateTime.Now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? DateTime.Now.AddDays(2) : DateTime.Now;
        var flexToday =
            CreateTimeEntryForExistingTask(dayToRegisterFlexOn, 1M, 18);
        await Assert.ThrowsAsync<Exception>(async () => await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = flexToday.Date, Value = flexToday.Value, TaskId = flexToday.TaskId } }));
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterVacation_UserIsEmployed50Percent_CannotRegister7Point5HoursVacation()
    {
        _context.EmploymentRate.Add(new EmploymentRate
        {
            UserId = 1,
            Rate = 0.5M,
            FromDate = new DateTime(2022, 01, 01),
            ToDate = new DateTime(2022, 01, 08)
        });
        await _context.SaveChangesAsync();

        var timeRegistrationService = CreateTimeRegistrationService();
        var vacationEntry = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 03), 7.5M, 13); //Monday
        await Assert.ThrowsAsync<Exception>(async () => await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = vacationEntry.Date, Value = vacationEntry.Value, TaskId = vacationEntry.TaskId } }));
    }

    private TimeRegistrationService CreateTimeRegistrationService()
    {
        return new TimeRegistrationService(_options, _userContextMock.Object, CreateTaskUtils(),
            new TimeRegistrationStorage(_context), new DbContextScope(_context), new PayoutStorage(_context), new UserService(new UserRepository(_context)));
    }

    private TaskUtils CreateTaskUtils()
    {
        return new TaskUtils(new TaskStorage(_context), _options);
    }

    private Hours CreateTimeEntryWithCompensationRate(DateTime date, decimal value, decimal compensationRate,
        out int taskId)
    {
        taskId = new Random().Next(1000, 10000000);
        var task = new Task { Id = taskId, Project = 1, };
        _context.Task.Add(task);
        _context.CompensationRate.Add(new CompensationRate
        { TaskId = taskId, Value = compensationRate, FromDate = new DateTime(2021, 01, 01) });
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
