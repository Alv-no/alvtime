using System;
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
using Tests.UnitTests.TestUtils;
using Tests.UnitTests.Utils;
using Xunit;

namespace Tests.UnitTests.Payouts;

public class PayoutServiceTests
{
    private readonly AlvTime_dbContext _context;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<PayoutValidationService> _payoutValidationServiceMock;
    private readonly TimeRegistrationService _timeRegistrationService;
    private readonly DateAlvTime _dateAlvTime;

    public PayoutServiceTests()
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
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(System.Threading.Tasks.Task.FromResult(user));

        _dateAlvTime = new DateAlvTime();
        _timeRegistrationService = CreateTimeRegistrationService(_dateAlvTime);

        _payoutValidationServiceMock = new Mock<PayoutValidationService>(new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)),
            _timeRegistrationService, new PayoutStorage(_context, _dateAlvTime));
        _payoutValidationServiceMock.Setup(x => x.CheckForIncompleteDays(It.IsAny<GenericPayoutHourEntry>(), It.IsAny<int>())).Returns(System.Threading.Tasks.Task.FromResult(new List<Error>()));
        _payoutValidationServiceMock.CallBase = true;
    }

    [Fact]
    public async System.Threading.Tasks.Task GetRegisteredPayouts_Registered10Hours_10HoursRegistered()
    {
        var timeEntry1 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 17.5M, 1.0M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);
        var registerOvertimeResult = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2022, 12, 13),
            Hours = 10
        });

        Assert.True(registerOvertimeResult.IsSuccess);
        var registerOvertimeResponse = registerOvertimeResult.Value; 

        var payoutResult = await payoutService.GetRegisteredPayouts();

        Assert.True(payoutResult.IsSuccess);
        var registeredPayouts = payoutResult.Value; 
        Assert.Equal(10, registerOvertimeResponse.HoursBeforeCompensation);
        Assert.Equal(10, registeredPayouts.TotalHoursBeforeCompRate);
        Assert.Equal(10, registeredPayouts.TotalHoursAfterCompRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetRegisteredPayouts_Registered3Times_ListWith3Items()
    {
        var timeEntry1 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 17.5M, 1.0M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);

        await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 13),
            Hours = 3
        });
        await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 14),
            Hours = 3
        });
        await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 15),
            Hours = 4
        });

        var payoutResult = await payoutService.GetRegisteredPayouts();

        Assert.True(payoutResult.IsSuccess);
        var registeredPayouts = payoutResult.Value;
        Assert.Equal(3, registeredPayouts.Entries.Count());
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_CalculationCorrectForBeforeAndAfterCompRate()
    {
        var timeEntry1 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 10M, 2.0M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var timeEntry2 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 14), 17.5M, 0.5M, out _); //Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);

        var registeredPayoutResult = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 14),
            Hours = 10
        });

        Assert.True(registeredPayoutResult.IsSuccess);
        var registeredPayout = registeredPayoutResult.Value;
        Assert.Equal(5, registeredPayout.HoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_CalculationCorrectForBeforeAndAfterCompRate2()
    {
        var timeEntry1 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 8.5M, 1.5M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var timeEntry2 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 14), 12.5M, 0.5M, out _); //Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });

        var timeEntry3 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 15), 9.0M, 1.0M, out _); //Wednesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId } });

        var timeEntry4 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 16), 9.5M, 1.5M, out _); //Thursday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = timeEntry4.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);

        var registeredPayoutResult = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 16),
            Hours = 6
        });

        Assert.True(registeredPayoutResult.IsSuccess);
        var registeredPayout = registeredPayoutResult.Value;
        Assert.Equal(3.5M, registeredPayout.HoursAfterCompensation);
        Assert.Equal(6M, registeredPayout.HoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_NotEnoughOvertime_CannotRegisterPayout()
    {
        var timeEntry1 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 11.5M, 1.5M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);

        var registeredPayoutResult = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 13),
            Hours = 7
        });

        Assert.False(registeredPayoutResult.IsSuccess);
        Assert.True(registeredPayoutResult.Errors.Any());
        var error = registeredPayoutResult.Errors.First();
        Assert.Equal(ErrorCodes.RequestInvalidProperty, error.ErrorCode);
        Assert.Equal("Ikke nok tilgjengelige timer.", error.Description);
    } 

    [Fact]
    public async System.Threading.Tasks.Task Lock_payout()
    {
        var timeEntry1 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 17.5M, 1.0M, out _);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);
        await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = DateTime.Today,
            Hours = 10
        });

        var registeredPayoutsResult = await payoutService.GetRegisteredPayouts();
        Assert.True(registeredPayoutsResult.IsSuccess);
        var registeredPayouts = registeredPayoutsResult.Value;
        var genericPayoutEntry = registeredPayouts.Entries.First();
        Assert.True(genericPayoutEntry.Active);
        var lockedPaymentResult = await payoutService.LockPayments(DateTime.Now);
        Assert.True(lockedPaymentResult.IsSuccess);
        var lockedPayout = (await payoutService.GetRegisteredPayouts()).Value.Entries.First();
        Assert.False(lockedPayout.Active);
    }


    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_RegisteringPayoutBeforeWorkingOvertime_NoPayout()
    {
        var timeEntry1 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 11.5M, 1.5M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);

        var registeredPayoutResult = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 12),
            Hours = 1
        });

        Assert.False(registeredPayoutResult.IsSuccess);
        Assert.True(registeredPayoutResult.Errors.Any());
        var error = registeredPayoutResult.Errors.First();
        Assert.Equal(ErrorCodes.RequestInvalidProperty, error.ErrorCode);
        Assert.Equal("Ikke nok tilgjengelige timer.", error.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_WorkingOvertimeAfterPayout_OnlyConsiderOvertimeWorkedBeforePayout()
    {
        var timeEntry1 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 7.5M, 1.5M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var timeEntry2 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 3M, 0.5M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });

        var timeEntry3 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 1.5M, 1.0M, out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);
        await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 14), //Tuesday
            Hours = 4
        });

        var timeEntry4 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 15), 7.5M, 1.5M, out _); //Wednesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = timeEntry4.TaskId } });

        var timeEntry5 =
            CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 15), 2M, 0.5M, out _); //Wednesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry5.Date, Value = timeEntry5.Value, TaskId = timeEntry5.TaskId } });

        var availableHoursAtPayoutDate =
            await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 14));
        var payoutEntriesAtPayoutDate = availableHoursAtPayoutDate.Entries.Where(e => e.Hours < 0).GroupBy(
            hours => hours.CompensationRate,
            hours => hours,
            (cr, hours) => new
            {
                CompensationRate = cr,
                Hours = hours.Sum(h => h.Hours)
            });

        var availableHoursAfterPayoutDate = await _timeRegistrationService.GetAvailableOvertimeHoursNow();
        var payoutEntriesAfterPayoutDate = availableHoursAfterPayoutDate.Entries.Where(e => e.Hours < 0).GroupBy(
            hours => hours.CompensationRate,
            hours => hours,
            (cr, hours) => new
            {
                CompensationRate = cr,
                Hours = hours.Sum(h => h.Hours)
            });

        Assert.Equal(payoutEntriesAfterPayoutDate, payoutEntriesAtPayoutDate);
        Assert.Equal(0.5M, availableHoursAtPayoutDate.AvailableHoursAfterCompensation);
        Assert.Equal(1.5M, availableHoursAfterPayoutDate.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task CancelPayout_PayoutIsRegisteredInSameMonth_PayoutIsCanceled()
    {
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var timeEntry1 =
            CreateTimeEntryWithCompensationRate(new DateTime(currentYear, currentMonth, 02), 17.5M, 1.0M,
                out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var testProvider = new TestDateAlvTimeProvider();
        _dateAlvTime.Provider = testProvider;
        testProvider.OverriddenValue = new DateTime(currentYear, currentMonth, 05);

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService, _dateAlvTime);

        await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(currentYear, currentMonth, 02),
            Hours = 1
        });

        await payoutService.CancelPayout(new DateTime(currentYear, currentMonth, 02));

        var activePayoutsResult = await payoutService.GetRegisteredPayouts();
        Assert.True(activePayoutsResult.IsSuccess);
        var activePayouts = activePayoutsResult.Value;
        Assert.Empty(activePayouts.Entries);
    }

    [Fact]
    public async System.Threading.Tasks.Task CancelPayout_PayoutIsRegisteredPreviousMonth_PayoutIsLocked()
    {
        var currentYear = DateTime.Now.Year;
        var previousMonth = DateTime.Now.AddMonths(-1).Month;
        var yearToTest = previousMonth == 12 ? currentYear - 1 : currentYear;

        var timeEntry1 =
            CreateTimeEntryWithCompensationRate(new DateTime(yearToTest, previousMonth, 02), 17.5M, 1.0M,
                out _); //Monday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);

        await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(yearToTest, previousMonth, 02),
            Hours = 5
        });

        var cancelPayoutResult = await payoutService.CancelPayout(new DateTime(currentYear, previousMonth, 02));
        Assert.False(cancelPayoutResult.IsSuccess);
        Assert.True(cancelPayoutResult.Errors.Any());
        var error = cancelPayoutResult.Errors.First();
        Assert.Equal(ErrorCodes.MissingEntity, error.ErrorCode);
        Assert.Equal("Det er ingen aktive utbetalinger.", error.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_HasFutureFlex_ExceptionThrown()
    {
        var currentYear = DateTime.Now.Year;
        var currentMonth = DateTime.Now.Month;
        var currentDay = DateTime.Now.Day;
        var overtimeEntry =
            CreateTimeEntryWithCompensationRate(new DateTime(currentYear, currentMonth, currentDay), 12M, 1.5M, out _);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = overtimeEntry.Date, Value = overtimeEntry.Value, TaskId = overtimeEntry.TaskId } });

        var futureDayToRegisterFlexOn = DateTime.Now.AddDays(5).DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? DateTime.Now.AddDays(7) : DateTime.Now.AddDays(5);
        var futureFlex =
            CreateTimeEntryForExistingTask(TestDateUtils.GetFutureNonWeekendDay(), 1M, 18);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = futureFlex.Date, Value = futureFlex.Value, TaskId = futureFlex.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);
        var registeredPayoutResult = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(currentYear, currentMonth, currentDay),
            Hours = 1M
        });

        Assert.False(registeredPayoutResult.IsSuccess);
        Assert.True(registeredPayoutResult.Errors.Any());
        var error = registeredPayoutResult.Errors.First();
        Assert.Equal(ErrorCodes.InvalidAction, error.ErrorCode);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_HasIncompleteDaysBeforePayout_ExceptionThrown()
    {
        for (var i = 0; i < 10; i++)
        {
            var date = DateTime.Now.AddDays(-i);
            var incompleteTimeEntry =
                CreateTimeEntryWithCompensationRate(date, 5M, 1.5M, out _);
            await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = incompleteTimeEntry.Date, Value = incompleteTimeEntry.Value, TaskId = incompleteTimeEntry.TaskId } });
        }

        var payoutService = CreatePayoutServiceWithIncompleteDaysValidation(_timeRegistrationService);
        var registeredPayoutResult = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = DateTime.Now,
            Hours = 1M
        });

        Assert.False(registeredPayoutResult.IsSuccess);
        Assert.True(registeredPayoutResult.Errors.Any());
        var error = registeredPayoutResult.Errors.First();
        Assert.Equal(ErrorCodes.InvalidAction, error.ErrorCode);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_HasIncompleteDaysOnRedDayBeforePayout_PayoutRegistered()
    {
        var startDate = new DateTime(2021, 05, 24); //2. pinsedag
        for (var i = 1; i < 31; i++)
        {
            var date = startDate.AddDays(-i);
            var completeTimeEntry =
                CreateTimeEntryWithCompensationRate(date, 7.5M, 1.5M, out _);
            if (date.Day == 17)
            {
                completeTimeEntry.Value = 5;
            }

            await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = completeTimeEntry.Date, Value = completeTimeEntry.Value, TaskId = completeTimeEntry.TaskId } });
        }

        var payoutService = CreatePayoutServiceWithIncompleteDaysValidation(_timeRegistrationService);
        await payoutService.RegisterPayout(new GenericPayoutHourEntry { Date = startDate, Hours = 5 });

        var registeredPayoutsResult = await payoutService.GetRegisteredPayouts();
        Assert.True(registeredPayoutsResult.IsSuccess);
        Assert.Single(registeredPayoutsResult.Value.Entries);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_UserStartedLessThan30DaysAgoAndHasIncompleteDaysBeforeThat_PayoutRegistered()
    {
        _context.User.First(u => u.Id == 1).StartDate = DateTime.Now.AddDays(-10);
        await _context.SaveChangesAsync();

        for (var i = 0; i < 10; i++)
        {
            var date = DateTime.Now.AddDays(-i);
            var completeTimeEntry =
                CreateTimeEntryWithCompensationRate(date, 8M, 1.5M, out _);
            await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = completeTimeEntry.Date, Value = completeTimeEntry.Value, TaskId = completeTimeEntry.TaskId } });
        }

        var payoutService = CreatePayoutServiceWithIncompleteDaysValidation(_timeRegistrationService);
        var payoutResult = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = DateTime.Now,
            Hours = 1M
        });

        Assert.True(payoutResult.IsSuccess);
        Assert.Equal(1, payoutResult.Value.HoursBeforeCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_PayoutMadeOnThe9ThAndCurrentDateIsThe9Th_PayoutIsActive()
    {
        var mockProvider = new Mock<IDateAlvTimeProvider>();
        var specificDate = new DateTime(2024, 4, 9);
        mockProvider.Setup(p => p.Now).Returns(specificDate);

        var dateAlvTime = new DateAlvTime { Provider = mockProvider.Object };

        var timeEntry =
            CreateTimeEntryWithCompensationRate(dateAlvTime.Now, 12M, 1.5M, out _);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService, dateAlvTime);
        await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = dateAlvTime.Now,
            Hours = 1M
        });

        var payoutResult = await payoutService.GetRegisteredPayouts();

        Assert.True(payoutResult.IsSuccess);
        var payout = payoutResult.Value.Entries[0];
        Assert.True(payout.Active);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_PayoutMadeOnThe8ThAndCurrentDateIsThe7Th_PayoutIsInactive()
    {
        var mockProvider = new Mock<IDateAlvTimeProvider>();
        var specificDate = new DateTime(2024, 4, 7);
        mockProvider.Setup(p => p.Now).Returns(specificDate);

        var dateAlvTime = new DateAlvTime { Provider = mockProvider.Object };

        var timeEntry =
            CreateTimeEntryWithCompensationRate(new DateTime(2024, 4, 6), 12M, 1.5M, out _);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService, dateAlvTime);
        await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2024, 4, 6),
            Hours = 1M
        });

        var payoutResult = await payoutService.GetRegisteredPayouts();

        Assert.True(payoutResult.IsSuccess);
        var payout = payoutResult.Value.Entries[0];
        Assert.False(payout.Active);
    }

    private TimeRegistrationService CreateTimeRegistrationService(DateAlvTime dateAlvTime)
    {
        return new TimeRegistrationService(_options, _userContextMock.Object, CreateTaskUtils(),
            new TimeRegistrationStorage(_context), new DbContextScope(_context), new PayoutStorage(_context, dateAlvTime), new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)));
    }

    private PayoutService CreatePayoutServiceWithoutIncompleteDaysValidation(TimeRegistrationService timeRegistrationService, DateAlvTime dateAlvTime = null)
    {
        return new PayoutService(new PayoutStorage(_context, dateAlvTime ?? new DateAlvTime()), _userContextMock.Object,
            timeRegistrationService, _payoutValidationServiceMock.Object);
    }

    private PayoutService CreatePayoutServiceWithIncompleteDaysValidation(TimeRegistrationService timeRegistrationService, DateAlvTime dateAlvTime = null)
    {
        return new PayoutService(new PayoutStorage(_context, new DateAlvTime()), _userContextMock.Object,
            timeRegistrationService, new PayoutValidationService(new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)), timeRegistrationService, new PayoutStorage(_context, new DateAlvTime())));
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