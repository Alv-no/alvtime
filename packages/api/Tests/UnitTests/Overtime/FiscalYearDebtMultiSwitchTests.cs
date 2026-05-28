using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

// Tests for multi-switch salary model history (static→invoice→static→invoice, etc.).
// Requires SalaryModelHistory to reconstruct the full per-FY model sequence.
public class FiscalYearDebtMultiSwitchTests
{
    private readonly AlvTime_dbContext _context;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private TimeRegistrationService _service;

    // User starts Static, StartDate = 2020-01-02
    private static readonly DateTime Switch1Date = new(2022, 6, 1); // Static → Invoice
    private static readonly DateTime Switch2Date = new(2023, 6, 1); // Invoice → Static
    private static readonly DateTime Switch3Date = new(2024, 6, 1); // Static → Invoice

    private static readonly DateTime FY2022Date = new(2022, 7, 4);  // FY 2022-2023 (Invoice)
    private static readonly DateTime FY2023Date = new(2023, 7, 3);  // FY 2023-2024 (Static)
    private static readonly DateTime FY2024Date = new(2024, 7, 1);  // FY 2024-2025 (Invoice)

    public FiscalYearDebtMultiSwitchTests()
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

    [Fact]
    public async System.Threading.Tasks.Task MultiSwitch_ThreeWay_EachSwitchYearGetsDebt()
    {
        // First invoice stint (FY 2022-2023): earn 60h → after 50h debt = 10h
        await ApplySwitch(Switch1Date, SalaryModel.Static, SalaryModel.InvoiceBased);
        await UpsertInternalEntry(FY2022Date, 67.5M); // 7.5h normal + 60h OT

        // Switch back to Static (FY 2023-2024)
        await ApplySwitch(Switch2Date, SalaryModel.InvoiceBased, SalaryModel.Static);
        await UpsertInternalEntry(FY2023Date, 47.5M); // 40h internal (no debt while Static)

        // Second invoice stint (FY 2024-2025): earn 60h → another 50h debt
        await ApplySwitch(Switch3Date, SalaryModel.Static, SalaryModel.InvoiceBased);
        await UpsertInternalEntry(FY2024Date, 67.5M); // 60h OT

        // FY 2022-2023 (invoice): 60h FY-scoped → min(50,60)=50 deducted → +10h
        // FY 2023-2024 (static): 40h, no debt → +40h
        // FY 2024-2025 (invoice): 60h FY-scoped → min(50,60)=50 deducted → +10h
        // Net: 10 + 40 + 10 = 60h
        var available = await _service.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        Assert.Equal(60M, available.AvailableHoursBeforeCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task MultiSwitch_ThreeWay_EachSwitchYearGetsDebt_CurrentYearBelowThreshold()
    {
        // First invoice stint (FY 2022-2023): earn 60h → after 50h debt = 10h
        await ApplySwitch(Switch1Date, SalaryModel.Static, SalaryModel.InvoiceBased);
        await UpsertInternalEntry(FY2022Date, 67.5M); // 7.5h normal + 60h OT

        // Switch back to Static (FY 2023-2024)
        await ApplySwitch(Switch2Date, SalaryModel.InvoiceBased, SalaryModel.Static);
        await UpsertInternalEntry(FY2023Date, 47.5M); // 40h internal (no debt while Static)

        // Second invoice stint (FY 2024-2025): earn 30h — below the 50h threshold
        await ApplySwitch(Switch3Date, SalaryModel.Static, SalaryModel.InvoiceBased);
        await UpsertInternalEntry(FY2024Date, 37.5M); // 30h OT

        // FY 2022-2023 (invoice): 60h FY-scoped → min(50,60)=50 deducted → +10h
        // FY 2023-2024 (static): 40h, no debt → +40h
        // FY 2024-2025 (invoice): 30h FY-scoped → min(50,30)=30 deducted → 0h
        // Net: 10 + 40 + 0 = 50h
        var available = await _service.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        Assert.Equal(50M, available.AvailableHoursBeforeCompensation);
    }
    
    [Fact]
    public async System.Threading.Tasks.Task MultiSwitch_ThreeWay_EachSwitchYearGetsDebt_EarlierYearHasNegativeBalance()
    {
        // First invoice stint (FY 2022-2023): earn 0h → only 30h debt applied = 0h
        await ApplySwitch(Switch1Date, SalaryModel.Static, SalaryModel.InvoiceBased);
        await UpsertInternalEntry(FY2022Date, 37.5M); // 7.5h normal + 60h OT

        // Switch back to Static (FY 2023-2024)
        await ApplySwitch(Switch2Date, SalaryModel.InvoiceBased, SalaryModel.Static);
        await UpsertInternalEntry(FY2023Date, 47.5M); // 40h internal (no debt while Static)

        // Second invoice stint (FY 2024-2025): earn 60h → another 50h debt
        await ApplySwitch(Switch3Date, SalaryModel.Static, SalaryModel.InvoiceBased);
        await UpsertInternalEntry(FY2024Date, 67.5M); // 10h OT

        // FY 2022-2023 (invoice): 60h FY-scoped → min(50,30)=30 deducted → +0h
        // FY 2023-2024 (static): 40h, no debt → +40h
        // FY 2024-2025 (invoice): 60h FY-scoped current year → min(50, 60) = 50h deducted → +10h
        // Net: 10 + 40 + 0 = 50h
        var available = await _service.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        Assert.Equal(50M, available.AvailableHoursBeforeCompensation);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetMockUser(SalaryModel model, DateTime? lastSwitchDate)
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

    private async System.Threading.Tasks.Task ApplySwitch(DateTime switchDate, SalaryModel previousModel, SalaryModel newModel)
    {
        // Record in SalaryModelHistory
        _context.SalaryModelHistory.Add(new SalaryModelHistory
        {
            UserId = 1,
            SwitchDate = switchDate,
            PreviousModel = previousModel,
            NewModel = newModel
        });
        await _context.SaveChangesAsync();

        SetMockUser(newModel, switchDate);
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
