using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business;
using AlvTime.Business.Options;
using AlvTime.Business.Overtime;
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

// Tests for the invoice-based salary model fiscal year debt:
// - Each fiscal year (June 1 → May 31) on invoice-based, the first 50h of
//   internal/volunteer overtime do not count toward the time bank.
// - The debt resets each fiscal year; unfilled debt is wiped when the year ends.
// - Billable overtime is never affected.
public class FiscalYearDebtTests
{
    private readonly AlvTime_dbContext _context;
    private readonly TimeRegistrationService _timeRegistrationService;

    // Mondays inside fiscal year 2024 (June 1 2024 → May 31 2025)
    private static readonly DateTime FY2024Date = new(2024, 7, 1);
    // Monday inside fiscal year 2025 (June 1 2025 → May 31 2026)
    private static readonly DateTime FY2025Date = new(2025, 7, 7);

    public FiscalYearDebtTests()
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
        var options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(o => o.CurrentValue == entryOptions);

        var userContextMock = new Mock<IUserContext>();
        userContextMock.Setup(c => c.GetCurrentUser()).Returns(
            System.Threading.Tasks.Task.FromResult(new AlvTime.Business.Users.User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone",
                StartDate = new DateTime(2020, 01, 02),
                Oid = "12345678-1234-1234-1234-123456789012",
                SalaryModel = SalaryModel.InvoiceBased
            }));

        _timeRegistrationService = new TimeRegistrationService(options, userContextMock.Object,
            new TaskUtils(new TaskStorage(_context), options),
            new TimeRegistrationStorage(_context), new DbContextScope(_context),
            new PayoutStorage(_context, new DateAlvTime()),
            new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)));
    }

    // ── Ongoing invoice-based (no switch from static) ─────────────────────────

    [Fact]
    public async System.Threading.Tasks.Task InternalOvertime_BelowThreshold_NotCounted()
    {
        await UpsertInternalEntry(FY2024Date, 47.5M); // 7.5h normal + 40h OT

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        // Debt capped at earned: min(50, 40) = 40h deducted. Net: 40 − 40 = 0h.
        Assert.Equal(0M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(0M, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task InternalOvertime_ExactlyAtThreshold_NotCounted()
    {
        await UpsertInternalEntry(FY2024Date, 57.5M); // 50h OT — right at the limit

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        Assert.Equal(0M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(0M, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task InternalOvertime_AboveThreshold_OnlyExcessAvailable()
    {
        await UpsertInternalEntry(FY2024Date, 62.5M); // 55h OT → 5h above threshold

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        Assert.Equal(5M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(5M * CompensationRates.Internal, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task VolunteerOvertime_BelowThreshold_NotCounted()
    {
        await UpsertVolunteerEntry(FY2024Date, 47.5M); // 40h volunteer OT

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        // Debt capped at earned: 40h volunteer absorbed by 40h of 50h threshold → 0h net.
        Assert.Equal(0M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(0M, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task MixedVolunteerAndInternal_ThresholdAppliesToCombined_VolunteerConsumedFirst()
    {
        // 30h volunteer + 30h internal = 60h combined.
        // Threshold absorbs 30h volunteer + 20h internal = 50h total.
        // 10h internal remain in bank.
        await UpsertVolunteerEntry(FY2024Date, 37.5M);           // 30h volunteer OT
        await UpsertInternalEntry(FY2024Date.AddDays(7), 37.5M); // 30h internal OT

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(8));

        Assert.Equal(10M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(10M * CompensationRates.Internal, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task BillableOvertime_NeverAffectedByFiscalYearDebt()
    {
        await UpsertBillableEntry(FY2024Date, 9.5M); // 2h billable OT

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        // Billable hours are unaffected regardless of the internal/volunteer threshold
        Assert.Equal(2M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(2M * CompensationRates.BillableInvoiceModel, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task FiscalYearDebt_ResetsEachYear_UnfilledDebtWiped()
    {
        // FY2024 (complete): earn 55h → min(50,55)=50h deducted → +5h surplus
        await UpsertInternalEntry(FY2024Date, 62.5M);

        // FY2025 (current): earn 40h → min(50,40)=40h deducted → 0h
        await UpsertInternalEntry(FY2025Date, 47.5M);

        // Net: 5h + 0h = 5h
        var available = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2025Date.AddDays(1));

        Assert.Equal(5M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(5M * CompensationRates.Internal, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task FiscalYearDebt_DoesNotApplyToStaticModelUser()
    {
        // Switch the user to static (clear seeded InvoiceBased history) and rebuild a static-model service
        _context.SalaryModelHistory.RemoveRange(_context.SalaryModelHistory);
        await _context.SaveChangesAsync();

        var options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(o => o.CurrentValue == new TimeEntryOptions
        {
            SickDaysTask = 14, PaidHolidayTask = 13, UnpaidHolidayTask = 19,
            FlexTask = 18, StartOfOvertimeSystem = new DateTime(2020, 01, 01), AbsenceProject = 9
        });
        var staticContextMock = new Mock<IUserContext>();
        staticContextMock.Setup(c => c.GetCurrentUser()).Returns(
            System.Threading.Tasks.Task.FromResult(new AlvTime.Business.Users.User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone",
                StartDate = new DateTime(2020, 01, 02),
                Oid = "12345678-1234-1234-1234-123456789012",
                SalaryModel = SalaryModel.Static
            }));
        var staticService = new TimeRegistrationService(options, staticContextMock.Object,
            new TaskUtils(new TaskStorage(_context), options),
            new TimeRegistrationStorage(_context), new DbContextScope(_context),
            new PayoutStorage(_context, new DateAlvTime()),
            new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)));

        await staticService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = FY2024Date, Value = 47.5M, TaskId = 2 } }); // 40h internal OT

        var available = await staticService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        // Static users: all internal overtime counts immediately
        Assert.Equal(40M, available.AvailableHoursBeforeCompensation);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async System.Threading.Tasks.Task UpsertInternalEntry(DateTime date, decimal hours)
    {
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = date, Value = hours, TaskId = 2 } }); // Task 2 = Internal
    }

    private async System.Threading.Tasks.Task UpsertVolunteerEntry(DateTime date, decimal hours)
    {
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = date, Value = hours, TaskId = 7 } }); // Task 7 = Volunteer
    }

    private async System.Threading.Tasks.Task UpsertBillableEntry(DateTime date, decimal hours)
    {
        var taskId = new Random().Next(1000, 10000000);
        var task = new Task { Id = taskId, Project = 1, CompensationType = CompensationType.Billable };
        _context.Task.Add(task);
        await _context.SaveChangesAsync();
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = date, Value = hours, TaskId = taskId } });
    }
}
