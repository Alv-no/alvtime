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

namespace Tests.UnitTests.Overtime;

// Tests for HoursUntilBankingStarts: how many more internal/volunteer overtime hours
// an invoice-based user needs to earn in the current FY before they start accumulating
// in the bank. Always 0 for static users or when the 50h threshold is met/exceeded.
public class HoursUntilBankingStartsTests
{
    private readonly AlvTime_dbContext _context;
    private readonly TimeRegistrationService _timeRegistrationService;

    private static readonly DateTime FY2024Date = new(2024, 7, 1); // Monday, FY 2024-2025

    public HoursUntilBankingStartsTests()
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

    [Fact]
    public async System.Threading.Tasks.Task InvoiceUser_Below50hThreshold_ShowsRemainingHours()
    {
        await UpsertInternalEntry(FY2024Date, 37.5M); // 30h OT

        var result = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        Assert.Equal(20M, result.HoursUntilBankingStarts); // 50 - 30 = 20
    }

    [Fact]
    public async System.Threading.Tasks.Task InvoiceUser_ExactlyAtThreshold_ReturnsZero()
    {
        await UpsertInternalEntry(FY2024Date, 57.5M); // 50h OT

        var result = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        Assert.Equal(0M, result.HoursUntilBankingStarts);
    }

    [Fact]
    public async System.Threading.Tasks.Task InvoiceUser_AboveThreshold_ReturnsZero()
    {
        await UpsertInternalEntry(FY2024Date, 62.5M); // 55h OT

        var result = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        Assert.Equal(0M, result.HoursUntilBankingStarts);
    }

    [Fact]
    public async System.Threading.Tasks.Task StaticUser_AlwaysReturnsZero()
    {
        var dbUser = _context.User.Find(1)!;
        dbUser.SalaryModel = SalaryModel.Static;
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
                Id = 1, Email = "someone@alv.no", Name = "Someone",
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
            { new() { Date = FY2024Date, Value = 47.5M, TaskId = 2 } }); // 40h OT

        var result = await staticService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(1));

        Assert.Equal(0M, result.HoursUntilBankingStarts);
    }

    [Fact]
    public async System.Threading.Tasks.Task MixedVolunteerAndInternal_CombinedExceedsThreshold_ReturnsZero()
    {
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = FY2024Date, Value = 37.5M, TaskId = 7 } }); // 30h volunteer
        await UpsertInternalEntry(FY2024Date.AddDays(7), 37.5M); // 30h internal

        var result = await _timeRegistrationService.GetAvailableOvertimeHoursAtDate(FY2024Date.AddDays(8));

        Assert.Equal(0M, result.HoursUntilBankingStarts); // 30 + 30 = 60 > 50
    }

    private async System.Threading.Tasks.Task UpsertInternalEntry(DateTime date, decimal hours)
    {
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = date, Value = hours, TaskId = 2 } });
    }
}
