﻿using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business;
using AlvTime.Business.Options;
using AlvTime.Business.Payouts;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using AlvTime.Business.Utils;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests.UnitTests.TimeEntries;

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

        var user = new AlvTime.Business.Users.User
        {
            Id = 1,
            Email = "someone@alv.no",
            Name = "Someone",
            Oid = "12345678-1234-1234-1234-123456789012"
        };

        _timeRegistrationService = CreateTimeRegistrationService();

        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(System.Threading.Tasks.Task.FromResult(user));

        var payoutValidationServiceMock = new Mock<PayoutValidationService>(new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)),
            _timeRegistrationService, new PayoutStorage(_context, new DateAlvTime()));
        payoutValidationServiceMock.Setup(x => x.CheckForIncompleteDays(It.IsAny<GenericPayoutHourEntry>(), It.IsAny<int>())).Returns(System.Threading.Tasks.Task.FromResult(new List<Error>()));
        payoutValidationServiceMock.CallBase = true;
        _payoutService = new PayoutService(new PayoutStorage(_context, new DateAlvTime()), _userContextMock.Object,
            _timeRegistrationService, payoutValidationServiceMock.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertTimeEntry_FlexBeforeARegisteredPayoutDate_ShouldNotBeAbleToFlex()
    {

        var timeEntry1 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 19.5M, 1.5M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

        await _payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 14),
            Hours = 1
        });

        var flexTimeEntry = CreateTimeEntryForExistingTask(new DateTime(2021, 12, 13), 1, 18);
        var timeEntryResult = await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId } });

        Assert.False(timeEntryResult.IsSuccess);
        Assert.True(timeEntryResult.Errors.Any());
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertTimeEntry_EarningOvertimeBeforeRegisteredPayoutDate_ShouldNotBeAbleToRegisterOvertime()
    {
        var timeEntry1 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 19.5M, 1.5M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

        await _payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 14),
            Hours = 1
        });

        var timeEntry2 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 08), 19.5M, 1.5M, out _); //Monday
        var timeEntryResult = await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });

        Assert.False(timeEntryResult.IsSuccess);
        Assert.True(timeEntryResult.Errors.Any());
    }

    [Fact]
    public async System.Threading.Tasks.Task UpsertTimeEntry_RecordedVacationOnChristmas_CannotRegisterVacationOnRedDay()
    {
        var vacationEntry =
            CreateTimeEntryForExistingTask(new DateTime(2021, 12, 24), 7.5M, 13);

        var timeEntryResult = await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = vacationEntry.Date, Value = vacationEntry.Value, TaskId = vacationEntry.TaskId}});

        Assert.False(timeEntryResult.IsSuccess);
        Assert.True(timeEntryResult.Errors.Any());
        Assert.True(timeEntryResult.Errors.First().Description.Equals("Du trenger ikke registrere fravær på en rød dag."));
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

        await _payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2022, 01, 28), //Friday
            Hours = 2.5M
        });

        var timeEntry3 =
            CreateTimeEntryForExistingTask(new DateTime(2022, 01, 28), 2M, taskId2); //Friday
        var timeEntryResult = await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId } });

        Assert.False(timeEntryResult.IsSuccess);
        Assert.True(timeEntryResult.Errors.Any());
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
        var timeEntryResult = await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = vacationEntry.Date, Value = vacationEntry.Value, TaskId = vacationEntry.TaskId } });

        Assert.False(timeEntryResult.IsSuccess);
        Assert.True(timeEntryResult.Errors.Any());
    }
    
    [Fact]
    public async System.Threading.Tasks.Task RegisterFlexAndHasImposedOvertime_ImposedOvertimeIsSubtractedLast()
    {
        var timeRegistrationService = CreateTimeRegistrationService();
        var timeEntry1 = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 03), 12.5M, 1); //Monday
        var timeEntry2 = CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 04), 12.5M, 2.0M, out var taskId); //Tuesday
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = taskId } });
        
        var flexTimeEntry = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 05), 6, 18);
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId } });

        var overTime = await timeRegistrationService.GetAvailableOvertimeHoursNow();
        Assert.Equal(4, overTime.AvailableHoursBeforeCompensation);
        Assert.Equal(8, overTime.AvailableHoursAfterCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task RegisterFlexAndHasImposedOvertime_ImposedOvertimeIsSubtractedLast2()
    {
        var timeRegistrationService = CreateTimeRegistrationService();
        var timeEntry1 = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 03), 8.5M, 1); //Monday
        var timeEntry2 = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 04), 8.5M, 2); //Tuesday
        var timeEntry3 = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 05), 8.5M, 3); //Wednesday
        var timeEntry4 = CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 06), 8.5M, 2.0M, out var taskId); //Thursday
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId } });
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = taskId } });
        
        var flexTimeEntry = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 07), 3, 18);
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId } });

        var overTime = await timeRegistrationService.GetAvailableOvertimeHoursNow();
        Assert.Equal(1, overTime.AvailableHoursBeforeCompensation);
        Assert.Equal(2, overTime.AvailableHoursAfterCompensation);
    }
        
    [Fact]
    public async System.Threading.Tasks.Task RegisterComment_TimeEntryIsLockedBecauseOfPayout_CommentStillUpdated()
    {
        var timeRegistrationService = CreateTimeRegistrationService();
        var timeEntry1 = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 03), 9.5M, 1); //Monday
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId, Comment = "Kommentar"} });

        await _payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2022, 01, 07), //Friday
            Hours = 1
        });
        
        var timeEntryResult = await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry1.Date, Value = 7.5M, TaskId = timeEntry1.TaskId, Comment = "Oppdatert kommentar"} });
        var timeEntry = _context.Hours.First(entry => entry.Date == timeEntry1.Date);

        Assert.True(timeEntryResult.Errors.Any());
        var error = timeEntryResult.Errors.First();
        Assert.Equal("Du har registrert en utbetaling som vil bli påvirket av denne timeføringen. Slett utbetalingen eller kontakt en admin for å få endret timene dine.", error.Description);
        Assert.Equal("Oppdatert kommentar", timeEntry.Comment);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task RegisterCommentWithZeroHours_TimeEntryDoesNotExistAndPayoutExists_CommentStillCreated()
    {
        var timeRegistrationService = CreateTimeRegistrationService();
        var timeEntry1 = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 03), 9.5M, 1); //Monday
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId, Comment = "Kommentar"} });

        await _payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2022, 01, 07), //Friday
            Hours = 1
        });
        
        var timeEntry2 = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 04), 0, 1); //Tuesday
        var timeEntryResult = await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId, Comment = "Kommentar 2"} });
        var timeEntry = _context.Hours.First(entry => entry.Date == timeEntry2.Date);

        Assert.False(timeEntryResult.Errors.Any());
        Assert.Equal("Kommentar 2", timeEntry.Comment);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task RegisterCommentWithNormalHours_TimeEntryDoesNotExistAndPayoutExists_CommentStillCreated()
    {
        var timeRegistrationService = CreateTimeRegistrationService();
        var timeEntry1 = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 03), 9.5M, 1); //Monday
        await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId, Comment = "Kommentar"} });

        await _payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2022, 01, 07), //Friday
            Hours = 1
        });
        
        var timeEntry2 = CreateTimeEntryForExistingTask(new DateTime(2022, 01, 04), 7.5M, 1); //Tuesday
        var timeEntryResult = await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId, Comment = "Kommentar 2"} });
        var timeEntry = _context.Hours.First(entry => entry.Date == timeEntry2.Date);

        Assert.False(timeEntryResult.Errors.Any());
        Assert.Equal("Kommentar 2", timeEntry.Comment);
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
