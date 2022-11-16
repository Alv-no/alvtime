using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Xunit;
using Task = AlvTime.Persistence.DatabaseModels.Task;

namespace Tests.UnitTests.Overtime;

public class OvertimeTests
{
    private readonly AlvTime_dbContext _context;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<PayoutValidationService> _payoutValidationServiceMock;
    private readonly TimeRegistrationService _timeRegistrationService;

    public OvertimeTests()
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
        
        _payoutValidationServiceMock = new Mock<PayoutValidationService>(new UserService(new UserRepository(_context)),
            _timeRegistrationService, new PayoutStorage(_context));
        _payoutValidationServiceMock.Setup(x => x.CheckForIncompleteDays(It.IsAny<GenericHourEntry>(), It.IsAny<int>())).Returns(System.Threading.Tasks.Task.FromResult(System.Threading.Tasks.Task.CompletedTask));
        _payoutValidationServiceMock.CallBase = true;
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_WorkedRegularDay_NoOvertime()
    {
        var dateToTest = new DateTime(2021, 12, 13); //Monday
        var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 7.5M, 1); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});

        var earnedOvertime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            {StartDate = dateToTest, EndDate = dateToTest});
        Assert.Empty(earnedOvertime);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_Worked9AndAHalfHoursOnWeekday_2HoursOvertime()
    {
        var dateToTest = new DateTime(2021, 12, 13); //Monday

        var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 9.5M, 1); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});

        var earnedOvertime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            {StartDate = dateToTest, EndDate = dateToTest});

        Assert.Single(earnedOvertime);
        Assert.Equal(2, earnedOvertime.First().Value);
        Assert.Equal(1.0M, earnedOvertime.First().CompensationRate);
        Assert.Equal(1, earnedOvertime.First().UserId);
        Assert.Equal(dateToTest, earnedOvertime.First().Date);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_WorkedOvertimeWithDifferentCompRatesOnWeekday_CorrectOvertimeWithCompRates()
    {
        var dateToTest = new DateTime(2021, 12, 13); //Monday
        var timeEntry1 = await CreateTimeEntryWithCompensationRate(dateToTest, 9.5M, 1.5M);
        var timeEntry2 = await CreateTimeEntryWithCompensationRate(dateToTest, 1M, 1.0M);
        var timeEntry3 = await CreateTimeEntryWithCompensationRate(dateToTest, 1M, 0.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId}});

        var earnedOvertime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            {StartDate = dateToTest, EndDate = dateToTest});

        Assert.Equal(3, earnedOvertime.Count);
        Assert.Equal(4, earnedOvertime.Sum(ot => ot.Value));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_Worked2HoursOnASaturday_2HoursOvertime()
    {
        var dateToTest = new DateTime(2021, 12, 11); //Saturday
        var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 2M, 1);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});

        var earnedOvertime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            {StartDate = dateToTest, EndDate = dateToTest});

        Assert.Single(earnedOvertime);
        Assert.Equal(2, earnedOvertime.Sum(ot => ot.Value));
        Assert.Equal(1.0M, earnedOvertime.First().CompensationRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_Worked2HoursOnARedDay_2HoursOvertime()
    {
        var dateToTest = new DateTime(2021, 04, 01); //Skjaertorsdag
        var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 2M, 1);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});

        var earnedOvertime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            {StartDate = dateToTest, EndDate = dateToTest});

        Assert.Single(earnedOvertime);
        Assert.Equal(2, earnedOvertime.Sum(ot => ot.Value));
        Assert.Equal(1.0M, earnedOvertime.First().CompensationRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_RegisteredVacationOnSaturday_NoOvertime()
    {
        var dateToTest = new DateTime(2021, 12, 11); //Saturday
        var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 7.5M, 14);
        await Assert.ThrowsAsync<Exception>(async () => await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}}));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_Registered10HoursVacationOnWeekday_NoOvertime()
    {
        var dateToTest = new DateTime(2021, 12, 13); //Monday
        var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 10M, 14);
        await Assert.ThrowsAsync<Exception>(async () => await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}}));
    }

    [Fact]
    public async System.Threading.Tasks.Task
        GetEarnedOvertime_WorkedOvertimeWithDifferentCompRatesOnWeekdayThenChangedEntries_CorrectOvertimeWithCompRates()
    {
        var dateToTest = new DateTime(2021, 12, 13); //Monday
        var timeEntry1 = await CreateTimeEntryWithCompensationRate(dateToTest, 9.5M, 1.5M);
        var timeEntry2 = await CreateTimeEntryWithCompensationRate(dateToTest, 1M, 1.0M);
        var timeEntry3 = await CreateTimeEntryWithCompensationRate(dateToTest, 1M, 0.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId}});

        var earnedOvertimeOriginal = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            {StartDate = dateToTest, EndDate = dateToTest});

        var timeEntry4 = CreateTimeEntryForExistingTask(dateToTest, 8M, timeEntry1.taskId);
        var timeEntry5 = CreateTimeEntryForExistingTask(dateToTest, 1.5M, timeEntry3.taskId);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = timeEntry4.TaskId}});
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry5.Date, Value = timeEntry5.Value, TaskId = timeEntry5.TaskId}});

        var earnedOvertimeUpdated = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            {StartDate = dateToTest, EndDate = dateToTest});

        Assert.NotEqual(earnedOvertimeOriginal, earnedOvertimeUpdated);
        Assert.Equal(3, earnedOvertimeOriginal.Count);
        Assert.Equal(3, earnedOvertimeUpdated.Count);
        Assert.Equal(3, earnedOvertimeUpdated.Sum(ot => ot.Value));
        Assert.Equal(4.5M, earnedOvertimeOriginal.Sum(ot => ot.Value * ot.CompensationRate));
        Assert.Equal(2.5M, earnedOvertimeUpdated.Sum(ot => ot.Value * ot.CompensationRate));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_WorkedOvertimeOverSeveralDaysAndChangedOneDay_CorrectOvertimeWithCompRates()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 9.5M, 1.5M); //Monday 
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 8M, 1.0M); //Tuesday
        var timeEntry3 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 8), 11M, 0.5M); //Wednesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId}});

        var timeEntry4 = CreateTimeEntryForExistingTask(new DateTime(2021, 12, 7), 8.5M, timeEntry2.taskId);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = timeEntry4.TaskId}});

        var earnedOvertime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            {StartDate = new DateTime(2021, 12, 6), EndDate = new DateTime(2021, 12, 8)});

        Assert.Equal(3, earnedOvertime.Count);
        Assert.Equal(6.5M, earnedOvertime.Sum(ot => ot.Value));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAvailableHours_Worked9AndAHalfHoursWith1AndAHalfCompRate_2HoursBeforeComp3HoursAfterComp()
    {
        var dateToTest = new DateTime(2021, 12, 6);
        var timeEntry1 = await CreateTimeEntryWithCompensationRate(dateToTest, 9.5M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Single(availableHours.Entries);
        Assert.Equal(2, availableHours.AvailableHoursBeforeCompensation);
        Assert.Equal(3, availableHours.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTimeEntries_UpdateOvertimeFails_NoHoursRegistered()
    {
        var sqliteContext = new AlvTimeDbContextBuilder(true)
            .WithCustomers()
            .WithProjects()
            .WithTasks()
            .WithLeaveTasks()
            .WithUsers()
            .CreateDbContext();
        var mockRepo = new Mock<TimeRegistrationStorage>(sqliteContext);
        mockRepo.Setup(mr => mr.DeleteOvertimeOnDate(It.IsAny<DateTime>(), It.IsAny<int>()))
            .Throws(new Exception());
        mockRepo.CallBase = true;
        var timeRegistrationService = new TimeRegistrationService(_options, _userContextMock.Object,
            CreateTaskUtils(), mockRepo.Object, new DbContextScope(sqliteContext), new PayoutStorage(sqliteContext), new UserService(new UserRepository(sqliteContext)));
        var dateToTest = new DateTime(2021, 12, 13); //Monday
        var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 10M, 1);
        try
        {
            await timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});
        }
        catch (Exception)
        {
            var earnedOvertime = await timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = dateToTest, EndDate = dateToTest});

            Assert.Empty(sqliteContext.Hours);
            Assert.Empty(earnedOvertime);
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_Worked7AndAHalfHours_NoOvertime()
    {
        var dateToTest = new DateTime(2021, 12, 6); //Monday
        var timeEntry1 = await CreateTimeEntryWithCompensationRate(dateToTest, 7.5M, 1.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(0, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_Worked10HoursDay1And5HoursDay2NoFlexRecorded_2AndAHalfOvertime()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 10M, 1.0M); // Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 5M, 1.0M); // Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(2.5M, availableHours.AvailableHoursBeforeCompensation);
        Assert.Equal(2.5M, availableHours.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_Worked5HoursBillableAnd5Hours0Point5CompRate_1Point25Overtime()
    {
        var dateToTest = new DateTime(2021, 12, 6); // Monday
        var timeEntry1 = await CreateTimeEntryWithCompensationRate(dateToTest, 5M, 1.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var timeEntry2 = await CreateTimeEntryWithCompensationRate(dateToTest, 5M, 0.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(1.25M, availableHours.AvailableHoursAfterCompensation);
        Assert.Equal(2.5M, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_Worked9P5HoursBillableAnd1Point5CompRate_3HoursOvertime()
    {
        var dateToTest = new DateTime(2021, 12, 6); // Monday
        var timeEntry1 = await CreateTimeEntryWithCompensationRate(dateToTest, 9.5M, 1.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(3M, availableHours.AvailableHoursAfterCompensation);
        Assert.Equal(2M, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task
        GetOvertime_Worked5Hours0Point5CompRateAnd7P5HoursBillableAnd5Hours1CompRate_7P5HoursAfterCompensation()
    {
        var dateToTest = new DateTime(2021, 12, 6); // Monday
        var timeEntry1 = await CreateTimeEntryWithCompensationRate(dateToTest, 5M, 0.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var timeEntry2 = await CreateTimeEntryWithCompensationRate(dateToTest, 5M, 1.0M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var timeEntry3 = await CreateTimeEntryWithCompensationRate(dateToTest, 7.5M, 1.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(7.5M, availableHours.AvailableHoursAfterCompensation);
        Assert.Equal(10M, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_Worked10Hours0Point5CompRateAnd10HoursBillableAnd10Hours1CompRate_7P5Overtime()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 10M, 0.5M); // Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 10M, 1.0M); // Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var timeEntry3 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 8), 10M, 1.5M); // Wednesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(7.5M, availableHours.AvailableHoursAfterCompensation);
        Assert.Equal(7.5M, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_OvertimeAndTimeOff_0Overtime()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 10M, 1.5M); // Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 5M, 1.5M); // Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var flexTimeEntry =
            CreateTimeEntryForExistingTask(new DateTime(2021, 12, 7), 2.5M, 18); // Wednesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(0M, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_OvertimeAndRegisteredPayout_5OvertimeLeft()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 17.5M, 1.0M); // Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var payoutService = CreatePayoutService(_timeRegistrationService);
        await payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2021, 12, 07), // Tuesday
            Hours = 5
        });

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();
            
        Assert.Equal(5M, availableHours.AvailableHoursBeforeCompensation);
        Assert.Equal(5M, availableHours.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_OvertimeAndRegisteredPayoutVariousCompRates_10OvertimeLeft()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 17.5M, 1.0M); // Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 12.5M, 1.5M); // Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});
            
        var timeEntry3 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 9M, 0.5M); // Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId}});

        var payoutService = CreatePayoutService(_timeRegistrationService);
        var registeredPayout = await payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2021, 12, 07), // Wednesday
            Hours = 11
        });

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();
            
        Assert.Equal(15.5M, availableHours.AvailableHoursAfterCompensation);
        Assert.Equal(13M, availableHours.AvailableHoursBeforeCompensation);
        Assert.Equal(6.5M, registeredPayout.HoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_RegisterOvertimeSameDateAsStartDate_5OvertimeAfterComp()
    {
        _context.User.First().StartDate = new DateTime(2021, 12, 6);
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 12.5M, 1.0M); // Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
            
        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(5M, availableHours.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_FlexingBeforeWorkingWithHighCompRate_WillNotSpendHighCompRateWhenFlexing()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 8.5M, 1.0M); // Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 6.5M, 1.0M); // Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var flexTimeEntry = CreateTimeEntryForExistingTask(new DateTime(2021, 12, 7), 1.0M, 18); // Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId}});

        var timeEntry3 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 8), 8.5M, 1.5M); // Wednesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(1.5M, availableHours.AvailableHoursAfterCompensation);
        Assert.Equal(1.0M, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_Worked2HoursOnKristiHimmelfart_3HoursInOvertimeAfterComp()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2020, 05, 21), 2M, 1.5M); // Kr.Himmelfart
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(3M, availableHours.AvailableHoursAfterCompensation);
        Assert.Equal(2M, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_Worked2HoursOnKristiHimmelfartAndMay17_6HoursInOvertime()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2020, 05, 21), 2M, 1.5M); // Kr.Himmelfart - Thursday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
            
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 05, 17), 2M, 1.5M); // 17 May - Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(6M, availableHours.AvailableHoursAfterCompensation);
        Assert.Equal(4M, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_Worked10HoursOnWorkdayAnd1HourWeekend_6P5HoursOvertimeAfterComp()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 17), 10.5M, 1.5M); // Friday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
            
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 18), 4M, 0.5M); // Saturday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(6.5M, availableHours.AvailableHoursAfterCompensation);
        Assert.Equal(7M, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_1May2020AndSecondPinseDag2021_5HoursOvertimeAfterComp()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2020, 05, 01), 2M, 1.5M); // 1st May - Friday 
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
            
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 05, 24), 4M, 0.5M); // 2nd Pinseday - Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(5M, availableHours.AvailableHoursAfterCompensation);
        Assert.Equal(6M, availableHours.AvailableHoursBeforeCompensation);
    }
        
    [Fact]
    public async System.Threading.Tasks.Task GetOvertimeAtDate_OnlyGetsOvertimeEarnedAtInputDate()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 12.5M, 1.5M); // Monday 
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
            
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 16), 12.5M, 1.5M); // Thursday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var availableHoursAtFirstOvertimeWorked = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 14));
        var availableHoursAfterBothOvertimesWorked = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(7.5M, availableHoursAtFirstOvertimeWorked.AvailableHoursAfterCompensation);
        Assert.Equal(10M, availableHoursAfterBothOvertimesWorked.AvailableHoursBeforeCompensation);
    }
        
    [Fact]
    public async System.Threading.Tasks.Task GetFlexhours_MultipleEmployees_FlexForSpecifiedEmployeeIsCalculated()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 12.5M, 1.5M); // Monday 
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
            
        var user2 = new AlvTime.Business.Models.User
        {
            Id = 2,
            Email = "someone_else@alv.no",
            Name = "Someone Else"
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(System.Threading.Tasks.Task.FromResult(user2));
            
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 12.5M, 1.5M); // Thursday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(5M, availableHours.AvailableHoursBeforeCompensation);
        Assert.Equal(7.5M, availableHours.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetFlexhoursToday_EntriesBeforeStartDate_OvertimeIsGiven()
    {
        var dateToTest = _context.User.First(user => user.Id == 1).StartDate.AddDays(-1);
            
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(dateToTest, 12.5M, 1.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.True(availableHours.AvailableHoursBeforeCompensation > 0);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetFlexhours_NotWorkedInWeekend_NoImpactOnOverTimeNorFlex()
    {
        // saturday and sunday:
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 18), 0M, 1.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 19), 0M, 1.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId}});

        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(0, availableHours.AvailableHoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterFlexEntry_FlexingMoreThanAvailable_CannotFlex()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 8.5M, 1.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId}});
        var flexTimeEntry =
            CreateTimeEntryForExistingTask(new DateTime(2021, 12, 14), 2M, 18);
        await Assert.ThrowsAsync<Exception>(async () => await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId}}));
            
        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(1M, availableHours.AvailableHoursBeforeCompensation);
        Assert.Equal(1.5M, availableHours.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterFlexEntry_FlexingBeforeRecordedHours_CannotFlex()
    { 
        var flexTimeEntry =
            CreateTimeEntryForExistingTask(new DateTime(2021, 12, 14), 1M, 18);
        await Assert.ThrowsAsync<Exception>(async () => await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId}}));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAvailableHours_Worked4HoursOnWeekend_4HoursOvertimeBeforeComp6AfterComp()
    {
        var timeEntry =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 18), 4M, 1.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = timeEntry.entry.Date, Value = timeEntry.entry.Value, TaskId = timeEntry.entry.TaskId}});
            
        var availableHours = await _timeRegistrationService.GetAvailableOvertimeHoursNow();
            
        Assert.Equal(4M, availableHours.AvailableHoursBeforeCompensation);
        Assert.Equal(6M, availableHours.AvailableHoursAfterCompensation);
    }
        
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_RegisteringVariousOvertimePayoutsAndFlex_CorrectOvertimeAtAllTimes()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 10.5M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });

        var availableOvertime1 =
            await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 13));
        Assert.Equal(4.5M, availableOvertime1.AvailableHoursAfterCompensation);
            
        var timeEntry2 =
            CreateTimeEntryForExistingTask(new DateTime(2021, 12, 13), 11.5M, timeEntry1.taskId); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });
            
        var flexTimeEntry =
            CreateTimeEntryForExistingTask(new DateTime(2021, 12, 14), 2, 18); //Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId } });
            
        var availableOvertime3 =
            await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 14));
        Assert.Equal(3M, availableOvertime3.AvailableHoursAfterCompensation);
            
        var payoutService = CreatePayoutService(_timeRegistrationService);
        await payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2021, 12, 15), //Wednesday
            Hours = 1
        });
            
        var availableOvertime4 =
            await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 15));
        Assert.Equal(1.5M, availableOvertime4.AvailableHoursAfterCompensation);

        var timeEntry3 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 16), 10.5M, 0.5M); //Thursday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId } });

        var availableOvertime5 =
            await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 16));
        Assert.Equal(3M, availableOvertime5.AvailableHoursAfterCompensation);
            
        await payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2021, 12, 17), //Friday
            Hours = 2
        });
            
        var availableOvertime6 =
            await _timeRegistrationService.GetAvailableOvertimeHoursNow();
        Assert.Equal(2M, availableOvertime6.AvailableHoursAfterCompensation);
    }
        
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_StartWithNegativeValue_GivesCorrectPayout()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), -5M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 12.5M, 0.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId } });
            
        var timeEntry3 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 04), 8.5M, 1.5M); //Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId } });
        var timeEntry4 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 04), 8M, 0.5M); //Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry4.entry.Date, Value = timeEntry4.entry.Value, TaskId = timeEntry4.taskId } });

        var payoutService = CreatePayoutService(_timeRegistrationService);
        var response = await payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2022, 01, 04), //Tuesday
            Hours = 9
        });
        Assert.Equal(5.5M, response.HoursAfterCompensation);
    }
        
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_RegisterPayoutBeforeFlexing_FlexShouldBeAllocatedToCorrectCompensationRates()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 17.5M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 04), 17.5M, 1.0M); //Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId } });
        var timeEntry3 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 05), 17.5M, 0.5M); //Wednesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId } });

        var payoutService = CreatePayoutService(_timeRegistrationService);
        await payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2022, 01, 06), //Thursday
            Hours = 10
        });
            
        var flexEntry =
            CreateTimeEntryForExistingTask(new DateTime(2022, 01, 07), 7.5M, 18); //Friday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = flexEntry.Date, Value = flexEntry.Value, TaskId = flexEntry.TaskId } });

        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 08));
        var flexOvertimeEntry = overtime.Entries.First(e => e.Hours == -7.5M);
        Assert.Equal(1.0M, flexOvertimeEntry.CompensationRate);
    }
        
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_RegisterFlexAndPayoutWithIncorrectCompRates_HistoricalBugDoesNotAffectNewCalculations()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 17.5M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 04), 17.5M, 1.0M); //Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId } });
        var timeEntry3 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 05), 17.5M, 0.5M); //Wednesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry3.entry.Date, Value = timeEntry3.entry.Value, TaskId = timeEntry3.taskId } });

        var payoutService = CreatePayoutService(_timeRegistrationService);
        await payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2022, 01, 06), //Thursday
            Hours = 10
        });

        var flexEntry =
            CreateTimeEntryForExistingTask(new DateTime(2022, 01, 07), 7.5M, 18); //Friday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = flexEntry.Date, Value = flexEntry.Value, TaskId = flexEntry.TaskId } });

        _context.PaidOvertime.Add(new PaidOvertime
        {
            HoursBeforeCompRate = 2.5M,
            HoursAfterCompRate = 2.5M,
            CompensationRate = 1.0M,
            User = 1,
            Date = new DateTime(2022, 01, 08)
        });
        _context.PaidOvertime.Add(new PaidOvertime
        {
            HoursBeforeCompRate = 10M,
            HoursAfterCompRate = 15M,
            CompensationRate = 1.5M,
            User = 1,
            Date = new DateTime(2022, 01, 08)
        });
        await _context.SaveChangesAsync();
            
        var timeEntry4 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 10), 8.5M, 0.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry4.entry.Date, Value = timeEntry4.entry.Value, TaskId = timeEntry4.taskId } });
            
        await payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2022, 01, 11), //Tuesday
            Hours = 1
        });
            
        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 12));
        Assert.Equal(0.0M, overtime.AvailableHoursBeforeCompensation);
    }
        
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_RegisterImposedOvertime_ImposedOvertimeIsPutOnTop()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 7.5M, 0.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var timeEntry2 =
            await CreateImposedOvertimeTimeEntry(new DateTime(2022, 01, 03), 2M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });

        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 04));
        Assert.Equal(2.0M, overtime.AvailableHoursBeforeCompensation);
        Assert.Equal(4.0M, overtime.AvailableHoursAfterCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_RegisterImposedOvertimeAndOrderPayout_ImposedOvertimeIsPutOnTop()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 7.5M, 0.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var imposedTimeEntry =
            await CreateImposedOvertimeTimeEntry(new DateTime(2022, 01, 03), 2M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = imposedTimeEntry.Date, Value = imposedTimeEntry.Value, TaskId = imposedTimeEntry.TaskId } });

        var payoutService = CreatePayoutService(_timeRegistrationService);
        var payout = await payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2022, 01, 03), //Thursday
            Hours = 2
        });
        
        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 04));
        Assert.Equal(0.0M, overtime.AvailableHoursBeforeCompensation);
        Assert.Equal(4.0M, payout.HoursAfterCompensation);
    }
    
    
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_RegisterImposedOvertime_ImposedOvertimeIsPutOnTop2()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 7.0M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 2.0M, 0.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId } });
        var imposedTimeEntry =
            await CreateImposedOvertimeTimeEntry(new DateTime(2022, 01, 03), 1M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = imposedTimeEntry.Date, Value = imposedTimeEntry.Value, TaskId = imposedTimeEntry.TaskId } });

        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 04));
        Assert.Equal(2.5M, overtime.AvailableHoursBeforeCompensation);
        Assert.Equal(2.75M, overtime.AvailableHoursAfterCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_RegisterImposedOvertimeWithPayout_ImposedOvertimeIsPutOnTop2()
    {
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 7.0M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 2.0M, 0.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId } });
        var imposedTimeEntry =
            await CreateImposedOvertimeTimeEntry(new DateTime(2022, 01, 03), 1M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = imposedTimeEntry.Date, Value = imposedTimeEntry.Value, TaskId = imposedTimeEntry.TaskId } });
        
        var payoutService = CreatePayoutService(_timeRegistrationService);
        var payout = await payoutService.RegisterPayout(new GenericHourEntry
        {
            Date = new DateTime(2022, 01, 03), //Thursday
            Hours = 2.5M
        });

        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 04));
        Assert.Equal(0M, overtime.AvailableHoursBeforeCompensation);
        Assert.Equal(2.75M, payout.HoursAfterCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_UserWorks50Percent_HoursOver3Point75GivesOvertime()
    {
        _context.EmploymentRate.Add(new EmploymentRate
        {
            UserId = 1,
            Rate = 0.5M,
            FromDate = new DateTime(2022, 01, 01),
            ToDate = new DateTime(2022, 01, 08)
        });
        await _context.SaveChangesAsync();
        
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 3.75M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 2.0M, 0.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId } });
        
        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 04));
        Assert.Equal(1M, overtime.AvailableHoursAfterCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_EmploymentRateNoLongerValid_RegularOvertime()
    {
        _context.EmploymentRate.Add(new EmploymentRate
        {
            UserId = 1,
            Rate = 0.5M,
            FromDate = new DateTime(2022, 01, 01),
            ToDate = new DateTime(2022, 01, 08)
        });
        await _context.SaveChangesAsync();
        
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 10), 3.75M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 10), 2.0M, 0.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId } });
        
        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 10));
        Assert.Equal(0M, overtime.AvailableHoursAfterCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_UserWorks70Percent_Over5Point25HoursGiveOvertime()
    {
        _context.EmploymentRate.Add(new EmploymentRate
        {
            UserId = 1,
            Rate = 0.7M,
            FromDate = new DateTime(2022, 01, 01),
            ToDate = new DateTime(2022, 01, 08)
        });
        await _context.SaveChangesAsync();
        
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 5.25M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 2.0M, 0.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId } });
        
        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 04));
        Assert.Equal(1M, overtime.AvailableHoursAfterCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_UserWorks70Percent_EarnsFlexToUseOver5Point25Hours()
    {
        _context.EmploymentRate.Add(new EmploymentRate
        {
            UserId = 1,
            Rate = 0.7M,
            FromDate = new DateTime(2022, 01, 01),
            ToDate = new DateTime(2022, 01, 08)
        });
        await _context.SaveChangesAsync();
        
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 6.25M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        
        var flexTimeEntry =
            CreateTimeEntryForExistingTask(new DateTime(2022, 01, 04), 1M, 18); // Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            {new() {Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId}});
        
        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 05));
        Assert.Equal(0M, overtime.AvailableHoursAfterCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_EmploymentRateIsInTheFuture_RegularOvertime()
    {
        _context.EmploymentRate.Add(new EmploymentRate
        {
            UserId = 1,
            Rate = 0.5M,
            FromDate = new DateTime(2022, 01, 10),
            ToDate = new DateTime(2022, 01, 20)
        });
        await _context.SaveChangesAsync();
        
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 3.75M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        var timeEntry2 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 03), 2.0M, 0.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.entry.Date, Value = timeEntry2.entry.Value, TaskId = timeEntry2.taskId } });
        
        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 04));
        Assert.Equal(0M, overtime.AvailableHoursAfterCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task GetOvertime_RecordsOvertimeOnDayEmploymentRateStarts_GetsCorrectOvertime()
    {
        _context.EmploymentRate.Add(new EmploymentRate
        {
            UserId = 1,
            Rate = 0.5M,
            FromDate = new DateTime(2022, 01, 10),
            ToDate = new DateTime(2022, 01, 20)
        });
        await _context.SaveChangesAsync();
        
        var timeEntry1 =
            await CreateTimeEntryWithCompensationRate(new DateTime(2022, 01, 10), 4M, 1.5M); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.entry.Date, Value = timeEntry1.entry.Value, TaskId = timeEntry1.taskId } });
        
        var overtime = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2022, 01, 10));
        Assert.Equal(0.375M, overtime.AvailableHoursAfterCompensation);
    }


    private TimeRegistrationService CreateTimeRegistrationService()
    {
        return new TimeRegistrationService(_options, _userContextMock.Object, CreateTaskUtils(),
            new TimeRegistrationStorage(_context), new DbContextScope(_context), new PayoutStorage(_context), new UserService(new UserRepository(_context)));
    }
        
    private PayoutService CreatePayoutService(TimeRegistrationService timeRegistrationService)
    {
        return new PayoutService(new PayoutStorage(_context), _userContextMock.Object,
            timeRegistrationService, _payoutValidationServiceMock.Object);
    }

    private TaskUtils CreateTaskUtils()
    {
        return new TaskUtils(new TaskStorage(_context), _options);
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

    private async Task<(Hours entry, int taskId)> CreateTimeEntryWithCompensationRate(DateTime date, decimal value, decimal compensationRate)
    {
        var taskId = new Random().Next(1000, 10000000);
        var task = new Task {Id = taskId, Project = 1,};
        _context.Task.Add(task);
        _context.CompensationRate.Add(new CompensationRate
            {TaskId = taskId, Value = compensationRate, FromDate = new DateTime(2019, 01, 01)});
        await _context.SaveChangesAsync();

        return (new Hours
        {
            User = 1,
            Date = date,
            Value = value,
            Task = task,
            TaskId = taskId
        }, taskId);
    }
    
    private async Task<Hours> CreateImposedOvertimeTimeEntry(DateTime date, decimal value)
    {
        var taskId = new Random().Next(1000, 10000000);
        var task = new Task {Id = taskId, Project = 1, Imposed = true};
        _context.Task.Add(task);
        _context.CompensationRate.Add(new CompensationRate
            {TaskId = taskId, Value = 2.0M, FromDate = new DateTime(2019, 01, 01)});
        await _context.SaveChangesAsync();

        return new Hours
        {
            User = 1,
            Date = date,
            Value = value,
            Task = task,
            TaskId = taskId
        };
    }
}