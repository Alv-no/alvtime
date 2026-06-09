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

// Tests for the fiscal year debt when a user switches salary models.
// Switch direction: Static → InvoiceBased (always in June, at the start of a FY).
// Pre-switch static hours are NOT subject to invoice debt — only FY-scoped hours count.
// All invoice FYs: min(50, FY-scoped earned) — debt never exceeds earnings in that FY.
public class FiscalYearDebtSwitchTests
{
    private readonly AlvTime_dbContext _context;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private TimeRegistrationService _service;

    // User starts Static; StartDate = 2 Jan 2020
    // Pre-switch earnings are registered in FY 2022-2023 (Static period)
    private static readonly DateTime PreSwitchDate = new(2022, 7, 4);  // Monday, FY 2022-2023 (Static)
    // Switch happens at the start of FY 2023-2024
    private static readonly DateTime SwitchDate    = new(2023, 6, 1);
    // Query/new-entry dates are in FY 2023-2024 (first Invoice FY)
    private static readonly DateTime AfterSwitchDate = new(2023, 7, 3); // Monday

    public FiscalYearDebtSwitchTests()
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
        _options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(o => o.CurrentValue == entryOptions);

        _userContextMock = new Mock<IUserContext>();
        SetMockUser(SalaryModel.Static, null);

        _service = BuildService();
    }

    // ── Static → Invoice switch ───────────────────────────────────────────────

    [Fact]
    public async System.Threading.Tasks.Task Switch_PreSwitchHours_UnaffectedByInvoiceDebt()
    {
        // Earn 40h internal while on Static (pre-switch, FY 2022-2023)
        await UpsertInternalEntry(PreSwitchDate, 47.5M); // 7.5h normal + 40h OT

        await SwitchToInvoiceBased();

        // FY 2023-2024: 0h FY-scoped earnings → no debt emitted; pre-switch 40h stay untouched
        var available = await _service.GetAvailableOvertimeHoursAtDate(AfterSwitchDate);

        Assert.Equal(40M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(40M * CompensationRates.Internal, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task Switch_PreSwitchHours_FullyPreservedInBank()
    {
        // Earn 60h internal while on Static (pre-switch, FY 2022-2023)
        await UpsertInternalEntry(PreSwitchDate, 67.5M); // 7.5h normal + 60h OT

        await SwitchToInvoiceBased();

        // FY 2023-2024: 0h FY-scoped earnings → no debt emitted; pre-switch 60h stay untouched
        var available = await _service.GetAvailableOvertimeHoursAtDate(AfterSwitchDate);

        Assert.Equal(60M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(60M * CompensationRates.Internal, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task Switch_InvoiceFY_NoHoursEarned_ZeroDebt()
    {
        // Switch with nothing in the bank
        await SwitchToInvoiceBased();

        // Earn 30h in the switch FY (FY-scoped)
        await UpsertInternalEntry(AfterSwitchDate, 37.5M); // 30h OT

        // FY 2023-2024 (current): 30h FY-scoped → min(50,30)=30h deducted → 0h
        var available = await _service.GetAvailableOvertimeHoursAtDate(AfterSwitchDate.AddDays(1));

        Assert.Equal(0M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(0M, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task Switch_BillablePreSwitchHours_NotAffectedByDebt()
    {
        // Earn 10h billable (Static rate 1.5x) before the switch — should be untouched by the 50h debt
        var taskId = new Random().Next(1000, 10000000);
        var task = new Task { Id = taskId, Project = 1, CompensationType = CompensationType.Billable };
        _context.Task.Add(task);
        await _context.SaveChangesAsync();
        await _service.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = PreSwitchDate, Value = 17.5M, TaskId = taskId } }); // 7.5h + 10h OT

        await SwitchToInvoiceBased();

        var available = await _service.GetAvailableOvertimeHoursAtDate(AfterSwitchDate);

        // Billable OT (10h @1.5) is unaffected; no internal/volunteer to trigger any debt
        Assert.Equal(10M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(10M * CompensationRates.BillableStaticModel, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task Switch_SecondInvoiceFY_DebtCappedAtEarned()
    {
        // Earn 60h internal before switch (pre-switch, FY 2022-2023 static) — stays untouched
        await UpsertInternalEntry(PreSwitchDate, 67.5M);

        await SwitchToInvoiceBased();

        // FY 2024-2025 (second invoice FY, current) — earn 40h internal
        var fy2Date = new DateTime(2024, 7, 1); // Monday, FY 2024-2025
        await UpsertInternalEntry(fy2Date, 47.5M); // 40h OT

        var available = await _service.GetAvailableOvertimeHoursAtDate(fy2Date.AddDays(1));

        // Pre-switch 60h: untouched (static model, no invoice debt)
        // FY 2023-2024 (switch, complete): 0h FY-scoped → no debt
        // FY 2024-2025 (current): 40h FY-scoped → min(50,40)=40h deducted → 0h net
        // Net: 60 + 40 − 40 = 60h
        Assert.Equal(60M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(60M * CompensationRates.Internal, available.AvailableHoursAfterCompensation);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetMockUser(SalaryModel model, DateTime? switchDate)
    {
        _userContextMock.Setup(c => c.GetCurrentUser()).Returns(
            System.Threading.Tasks.Task.FromResult(new AlvTime.Business.Users.User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone",
                StartDate = new DateTime(2020, 01, 02),
                Oid = "12345678-1234-1234-1234-123456789012",
                SalaryModel = model
            }));
        _service = BuildService();
    }

    private async System.Threading.Tasks.Task SwitchToInvoiceBased()
    {
        _context.SalaryModelHistory.Add(new SalaryModelHistory
        {
            UserId = 1,
            SwitchDate = SwitchDate,
            PreviousModel = SalaryModel.Static,
            NewModel = SalaryModel.InvoiceBased
        });
        await _context.SaveChangesAsync();

        SetMockUser(SalaryModel.InvoiceBased, SwitchDate);
    }

    private TimeRegistrationService BuildService() =>
        new(_options, _userContextMock.Object,
            new TaskUtils(new TaskStorage(_context), _options),
            new TimeRegistrationStorage(_context), new DbContextScope(_context),
            new PayoutStorage(_context, new DateAlvTime()),
            new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)));

    private async System.Threading.Tasks.Task UpsertInternalEntry(DateTime date, decimal hours)
    {
        await _service.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = date, Value = hours, TaskId = 2 } }); // Task 2 = Internal
    }
}
