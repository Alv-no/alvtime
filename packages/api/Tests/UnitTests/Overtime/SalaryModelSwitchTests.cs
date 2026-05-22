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
using Xunit;
using Task = AlvTime.Persistence.DatabaseModels.Task;

namespace Tests.UnitTests.Overtime;

public class SalaryModelSwitchTests
{
    private readonly AlvTime_dbContext _context;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<PayoutValidationService> _payoutValidationServiceMock;
    private readonly TimeRegistrationService _timeRegistrationService;

    // Dates used across tests — all Mondays to avoid weekend OT semantics
    private static readonly DateTime PreSwitchDay = new(2022, 01, 03);   // Monday
    private static readonly DateTime SwitchDate    = new(2022, 01, 10);   // Monday
    private static readonly DateTime PostSwitchDay = new(2022, 01, 17);   // Monday

    public SalaryModelSwitchTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .WithTasks()
            .WithLeaveTasks()
            .WithProjects()
            .WithStaticSalaryUsers()
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
        _userContextMock.Setup(c => c.GetCurrentUser()).Returns(
            System.Threading.Tasks.Task.FromResult(new AlvTime.Business.Users.User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone",
                Oid = "12345678-1234-1234-1234-123456789012"
            }));

        _timeRegistrationService = CreateTimeRegistrationService();

        _payoutValidationServiceMock = new Mock<PayoutValidationService>(
            new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)),
            _timeRegistrationService,
            new PayoutStorage(_context, new DateAlvTime()));
        _payoutValidationServiceMock
            .Setup(x => x.CheckForIncompleteDays(It.IsAny<GenericPayoutHourEntry>(), It.IsAny<int>()))
            .Returns(System.Threading.Tasks.Task.FromResult(new List<Error>()));
        _payoutValidationServiceMock.CallBase = true;
    }

    [Fact]
    public async System.Threading.Tasks.Task EarnedOvertimeBeforeSwitch_RetainsOriginalCompensationRate()
    {
        var billableEntry = await CreateBillableEntry(PreSwitchDay, 9.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = billableEntry.Date, Value = billableEntry.Value, TaskId = billableEntry.TaskId } });

        await SwitchToInvoiceBased(SwitchDate);

        var overtime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            { FromDateInclusive = PreSwitchDay, ToDateInclusive = PreSwitchDay });

        Assert.Single(overtime);
        Assert.Equal(2M, overtime.First().Value);
        Assert.Equal(CompensationRates.BillableStaticModel, overtime.First().CompensationRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task EarnedOvertimeAfterSwitch_UsesNewCompensationRate()
    {
        await SwitchToInvoiceBased(SwitchDate);

        var billableEntry = await CreateBillableEntry(PostSwitchDay, 9.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = billableEntry.Date, Value = billableEntry.Value, TaskId = billableEntry.TaskId } });

        var overtime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            { FromDateInclusive = PostSwitchDay, ToDateInclusive = PostSwitchDay });

        Assert.Single(overtime);
        Assert.Equal(2M, overtime.First().Value);
        Assert.Equal(CompensationRates.BillableInvoiceModel, overtime.First().CompensationRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task AvailableOvertime_AfterSwitch_IncludesBothPreAndPostSwitchEntries()
    {
        // Pre-switch: earn 2h @1.5
        var preSwitchEntry = await CreateBillableEntry(PreSwitchDay, 9.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = preSwitchEntry.Date, Value = preSwitchEntry.Value, TaskId = preSwitchEntry.TaskId } });

        await SwitchToInvoiceBased(SwitchDate);

        // Post-switch: earn 2h @1.4
        var postSwitchEntry = await CreateBillableEntry(PostSwitchDay, 9.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = postSwitchEntry.Date, Value = postSwitchEntry.Value, TaskId = postSwitchEntry.TaskId } });

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(4M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(
            2M * CompensationRates.BillableStaticModel + 2M * CompensationRates.BillableInvoiceModel,
            available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task FlexBalance_AfterSwitch_ExistingFlexUnaffected()
    {
        // Earn 2h @1.5, take 2h flex
        var billableEntry = await CreateBillableEntry(PreSwitchDay, 10.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = billableEntry.Date, Value = billableEntry.Value, TaskId = billableEntry.TaskId } });

        var flexDay = PreSwitchDay.AddDays(1); // Tuesday
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = flexDay, Value = 2M, TaskId = 18 } });

        var availableBeforeSwitch = await _timeRegistrationService.GetAvailableOvertimeHoursNow();
        Assert.Equal(1.5M, availableBeforeSwitch.AvailableHoursAfterCompensation);

        await SwitchToInvoiceBased(SwitchDate);

        var availableAfterSwitch = await _timeRegistrationService.GetAvailableOvertimeHoursNow();
        Assert.Equal(1.5M, availableAfterSwitch.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisteredPayout_AfterSwitch_ExistingPayoutUnaffected()
    {
        // Earn 4h @1.5 and register a payout of 2h
        var billableEntry = await CreateBillableEntry(PreSwitchDay, 11.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = billableEntry.Date, Value = billableEntry.Value, TaskId = billableEntry.TaskId } });

        var payoutService = CreatePayoutService();
        var payoutResult = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = PreSwitchDay.AddDays(1),
            Hours = 2
        });

        Assert.True(payoutResult.IsSuccess);
        var payoutBefore = payoutResult.Value;

        await SwitchToInvoiceBased(SwitchDate);

        var registeredPayoutsResult = await payoutService.GetRegisteredPayouts();
        Assert.True(registeredPayoutsResult.IsSuccess);
        var storedPayout = registeredPayoutsResult.Value.Entries.Single();

        Assert.Equal(payoutBefore.HoursBeforeCompensation, storedPayout.HoursBeforeCompRate);
        Assert.Equal(payoutBefore.HoursAfterCompensation, storedPayout.HoursAfterCompRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task NewPayout_AfterSwitch_CorrectMixedRateConsumption()
    {
        // Earn 2h @1.5 before switch
        var preSwitchEntry = await CreateBillableEntry(PreSwitchDay, 9.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = preSwitchEntry.Date, Value = preSwitchEntry.Value, TaskId = preSwitchEntry.TaskId } });

        await SwitchToInvoiceBased(SwitchDate);

        // Earn 2h @1.4 after switch
        var postSwitchEntry = await CreateBillableEntry(PostSwitchDay, 9.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = postSwitchEntry.Date, Value = postSwitchEntry.Value, TaskId = postSwitchEntry.TaskId } });

        var payoutService = CreatePayoutService();
        var result = await payoutService.RegisterPayout(new GenericPayoutHourEntry
        {
            Date = PostSwitchDay.AddDays(1),
            Hours = 4
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(4M, result.Value.HoursBeforeCompensation);
        // Lower rate (1.4) consumed first, then 1.5
        Assert.Equal(
            2M * CompensationRates.BillableInvoiceModel + 2M * CompensationRates.BillableStaticModel,
            result.Value.HoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTimeEntry_BeforeSalaryModelSwitch_Succeeds()
    {
        var entryDay = new DateTime(2022, 01, 10); // Monday

        // Create the initial time entry
        var billableEntry = await CreateBillableEntry(entryDay, 7.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = billableEntry.Date, Value = billableEntry.Value, TaskId = billableEntry.TaskId } });

        // Switch after the entry date
        await SwitchToInvoiceBased(new DateTime(2022, 01, 15));

        // Update the entry that predates the switch — should succeed
        var updateResult = await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = entryDay, Value = 9M, TaskId = billableEntry.TaskId } });

        Assert.True(updateResult.IsSuccess);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTimeEntry_BeforeSalaryModelSwitch_UsesPreSwitchCompensationRate()
    {
        // Earn 2h OT @1.5 (Static) before the switch
        var billableEntry = await CreateBillableEntry(PreSwitchDay, 9.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = billableEntry.Date, Value = billableEntry.Value, TaskId = billableEntry.TaskId } });

        await SwitchToInvoiceBased(SwitchDate);

        // Update the pre-switch entry — now earns 3h OT
        var updateResult = await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = PreSwitchDay, Value = 10.5M, TaskId = billableEntry.TaskId } });

        Assert.True(updateResult.IsSuccess);

        var overtime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            { FromDateInclusive = PreSwitchDay, ToDateInclusive = PreSwitchDay });

        Assert.Single(overtime);
        Assert.Equal(3M, overtime.First().Value);
        Assert.Equal(CompensationRates.BillableStaticModel, overtime.First().CompensationRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTimeEntry_AfterSalaryModelSwitch_Succeeds()
    {
        var entryDay = new DateTime(2022, 01, 17); // Monday, after switch date

        // Switch before registering the entry
        await SwitchToInvoiceBased(new DateTime(2022, 01, 10));

        var billableEntry = await CreateBillableEntry(entryDay, 7.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = billableEntry.Date, Value = billableEntry.Value, TaskId = billableEntry.TaskId } });

        // Update the same entry — should succeed since it is after the switch date
        var updateResult = await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = entryDay, Value = 9.5M, TaskId = billableEntry.TaskId } });

        Assert.True(updateResult.IsSuccess);
    }

    private async System.Threading.Tasks.Task SwitchToInvoiceBased(DateTime switchDate)
    {
        var dbUser = _context.User.First(u => u.Id == 1);
        dbUser.SalaryModel = SalaryModel.InvoiceBased;
        dbUser.LastSwitchedSalaryModel = switchDate;
        await _context.SaveChangesAsync();

        _userContextMock.Setup(c => c.GetCurrentUser()).Returns(
            System.Threading.Tasks.Task.FromResult(new AlvTime.Business.Users.User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone",
                Oid = "12345678-1234-1234-1234-123456789012",
                LastSwitchSalaryModel = switchDate
            }));
    }

    private TimeRegistrationService CreateTimeRegistrationService()
    {
        return new TimeRegistrationService(_options, _userContextMock.Object,
            new TaskUtils(new TaskStorage(_context), _options),
            new TimeRegistrationStorage(_context), new DbContextScope(_context),
            new PayoutStorage(_context, new DateAlvTime()),
            new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)));
    }

    private PayoutService CreatePayoutService()
    {
        return new PayoutService(new PayoutStorage(_context, new DateAlvTime()), _userContextMock.Object,
            _timeRegistrationService, _payoutValidationServiceMock.Object);
    }

    private async System.Threading.Tasks.Task<Hours> CreateBillableEntry(DateTime date, decimal hours)
    {
        var taskId = new Random().Next(1000, 10000000);
        var task = new Task { Id = taskId, Project = 1, CompensationType = CompensationType.Billable };
        await _context.Task.AddAsync(task);
        await _context.SaveChangesAsync();
        return new Hours { User = 1, Date = date, Value = hours, Task = task, TaskId = taskId };
    }
}
