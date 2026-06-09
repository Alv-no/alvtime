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

// Tests that GetTimeEntriesWithCompensationRate resolves the salary model from
// SalaryModelHistory rather than the binary "flip current model" toggle.
// The binary toggle breaks for multi-switch users (Static→Invoice→Static):
// editing a pre-first-switch entry incorrectly applies Invoice rate.
public class CompensationRateHistoryTests
{
    private readonly AlvTime_dbContext _context;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private TimeRegistrationService _service;

    private static readonly DateTime PreFirstSwitchDate = new(2021, 7, 5);  // Monday, FY 2021-2022 (Static)
    private static readonly DateTime Switch1Date         = new(2022, 6, 1);  // Static → Invoice
    private static readonly DateTime Switch2Date         = new(2023, 6, 1);  // Invoice → Static
    private static readonly DateTime AfterSwitch2Date    = new(2023, 7, 3);  // Monday, after both switches

    public CompensationRateHistoryTests()
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
    public async System.Threading.Tasks.Task MultiSwitch_EditPreSwitchBillableEntry_UsesOriginalStaticRate()
    {
        var taskId = new Random().Next(1000, 10000000);
        _context.Task.Add(new Task { Id = taskId, Project = 1, CompensationType = CompensationType.Billable });
        await _context.SaveChangesAsync();

        // Register 10h billable while on Static
        await _service.UpsertTimeEntry([new() { Date = PreFirstSwitchDate, Value = 17.5M, TaskId = taskId }]); // 7.5h + 10h OT

        // Switch Static → Invoice → Static
        await ApplySwitch(Switch1Date, SalaryModel.Static, SalaryModel.InvoiceBased);
        await ApplySwitch(Switch2Date, SalaryModel.InvoiceBased, SalaryModel.Static);

        // Edit the pre-switch entry — triggers GetTimeEntriesWithCompensationRate to recompute rate.
        // Binary toggle bug: current=Static, lastSwitch=Switch2Date, entry < lastSwitch → flips to Invoice (1.4×).
        // Correct behaviour: SalaryModelHistory shows entry date precedes first switch → Static (1.5×).
        await _service.UpsertTimeEntry([new() { Date = PreFirstSwitchDate, Value = 17.5M, TaskId = taskId }]);

        var available = await _service.GetAvailableOvertimeHoursAtDate(AfterSwitch2Date);

        Assert.Equal(10M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(10M * CompensationRates.BillableStaticModel, available.AvailableHoursAfterCompensation);
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
}
