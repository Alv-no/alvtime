using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business;
using AlvTime.Business.Options;
using AlvTime.Business.Overtime;
using AlvTime.Business.Payouts;
using AlvTime.Business.Tasks;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using AlvTime.Business.Utils;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Tests.UnitTests.TestUtils;
using Xunit;
using Task = AlvTime.Persistence.DatabaseModels.Task;

namespace Tests.UnitTests.Payouts;

public class InvoiceBasedPayoutServiceTests
{
    private readonly AlvTime_dbContext _context;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<PayoutValidationService> _payoutValidationServiceMock;
    private readonly TimeRegistrationService _timeRegistrationService;
    private readonly DateAlvTime _dateAlvTime;

    public InvoiceBasedPayoutServiceTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .WithTasks()
            .WithLeaveTasks()
            .WithProjects()
            .WithInvoiceBasedSalaryUsers()
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

        _payoutValidationServiceMock = new Mock<PayoutValidationService>(
            new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)),
            _timeRegistrationService,
            new PayoutStorage(_context, _dateAlvTime));
        _payoutValidationServiceMock.Setup(x => x.CheckForIncompleteDays(It.IsAny<GenericPayoutHourEntry>(), It.IsAny<int>()))
            .Returns(System.Threading.Tasks.Task.FromResult(new List<Error>()));
        _payoutValidationServiceMock.CallBase = true;
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_InvoiceBasedUser_BillableOvertime_CorrectRateApplied()
    {
        var monday = new DateTime(2021, 12, 13);
        var timeEntry = CreateTimeEntryWithCompensationRate(monday, 9.5M, CompensationType.Billable, out _);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);
        var result = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 14),
            Hours = 2
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(2M, result.Value.HoursBeforeCompensation);
        Assert.Equal(2M * CompensationRates.BillableInvoiceModel, result.Value.HoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterPayout_InvoiceBasedUser_InsufficientOvertime_ReturnsError()
    {
        var monday = new DateTime(2021, 12, 13);
        var timeEntry = CreateTimeEntryWithCompensationRate(monday, 9.5M, CompensationType.Billable, out _);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId } });

        var payoutService = CreatePayoutServiceWithoutIncompleteDaysValidation(_timeRegistrationService);
        var result = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = new DateTime(2021, 12, 14),
            Hours = 5
        });

        Assert.False(result.IsSuccess);
        Assert.True(result.Errors.Any());
        Assert.Equal(ErrorCodes.RequestInvalidProperty, result.Errors.First().ErrorCode);
    }

    private TimeRegistrationService CreateTimeRegistrationService(DateAlvTime dateAlvTime)
    {
        return new TimeRegistrationService(_options, _userContextMock.Object,
            new TaskUtils(new TaskStorage(_context), _options),
            new TimeRegistrationStorage(_context), new DbContextScope(_context),
            new PayoutStorage(_context, dateAlvTime),
            new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)));
    }

    private PayoutService CreatePayoutServiceWithoutIncompleteDaysValidation(TimeRegistrationService timeRegistrationService, DateAlvTime dateAlvTime = null)
    {
        return new PayoutService(new PayoutStorage(_context, dateAlvTime ?? new DateAlvTime()), _userContextMock.Object,
            timeRegistrationService, _payoutValidationServiceMock.Object);
    }

    private Hours CreateTimeEntryWithCompensationRate(DateTime date, decimal value, CompensationType compensationType, out int taskId)
    {
        taskId = new Random().Next(1000, 10000000);
        var task = new Task { Id = taskId, Project = 1, CompensationType = compensationType };
        _context.Task.Add(task);
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
}
