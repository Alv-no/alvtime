using System;
using System.Collections.Generic;
using System.Linq;
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

public class InvoiceBasedSalaryModelOvertimeTests
{
    private readonly AlvTime_dbContext _context;
    private readonly TimeRegistrationService _timeRegistrationService;

    public InvoiceBasedSalaryModelOvertimeTests()
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
        var options1 = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);

        var userContextMock = new Mock<IUserContext>();
        var user = new AlvTime.Business.Users.User
        {
            Id = 1,
            Email = "someone@alv.no",
            Name = "Someone",
            Oid = "12345678-1234-1234-1234-123456789012"
        };
        userContextMock.Setup(context => context.GetCurrentUser()).Returns(System.Threading.Tasks.Task.FromResult(user));

        _timeRegistrationService = new TimeRegistrationService(options1, userContextMock.Object,
            new TaskUtils(new TaskStorage(_context), options1),
            new TimeRegistrationStorage(_context), new DbContextScope(_context),
            new PayoutStorage(_context, new DateAlvTime()),
            new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_InvoiceBasedUser_BillableRegularDay_NoOvertime()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var entry = await CreateBillableEntry(date, 7.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = entry.Date, Value = entry.Value, TaskId = entry.TaskId } });

        var overtime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            { FromDateInclusive = date, ToDateInclusive = date });

        Assert.Empty(overtime);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_InvoiceBasedUser_BillableOvertime_RateIs1Point4()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var entry = await CreateBillableEntry(date, 9.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = entry.Date, Value = entry.Value, TaskId = entry.TaskId } });

        var overtime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            { FromDateInclusive = date, ToDateInclusive = date });

        Assert.Single(overtime);
        Assert.Equal(2, overtime.First().Value);
        Assert.Equal(CompensationRates.BillableInvoiceModel, overtime.First().CompensationRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_InvoiceBasedUser_InternalOvertime_RateIs1Point0()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var entry = await CreateInternalEntry(date, 10M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = entry.Date, Value = entry.Value, TaskId = entry.TaskId } });

        var overtime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            { FromDateInclusive = date, ToDateInclusive = date });

        Assert.Single(overtime);
        Assert.Equal(2.5M, overtime.First().Value);
        Assert.Equal(CompensationRates.Internal, overtime.First().CompensationRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_InvoiceBasedUser_VolunteerOvertime_RateIs0Point5()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var entry = await CreateVolunteerEntry(date, 10M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = entry.Date, Value = entry.Value, TaskId = entry.TaskId } });

        var overtime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            { FromDateInclusive = date, ToDateInclusive = date });

        Assert.Single(overtime);
        Assert.Equal(2.5M, overtime.First().Value);
        Assert.Equal(CompensationRates.Volunteer, overtime.First().CompensationRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_InvoiceBasedUser_ImposedBillableOvertime_RateIs2Point0()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var volunteerEntry = await CreateVolunteerEntry(date, 7.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = volunteerEntry.Date, Value = volunteerEntry.Value, TaskId = volunteerEntry.TaskId } });

        var imposedEntry = await CreateImposedEntry(date, 2M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = imposedEntry.Date, Value = imposedEntry.Value, TaskId = imposedEntry.TaskId } });

        var overtime = await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            { FromDateInclusive = date, ToDateInclusive = date });

        Assert.Single(overtime);
        Assert.Equal(2M, overtime.First().Value);
        Assert.Equal(CompensationRates.Imposed, overtime.First().CompensationRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEarnedOvertime_InvoiceBasedUser_MixedCompTypes_CorrectRatesForEach()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var billableEntry = await CreateBillableEntry(date, 9.5M);
        var internalEntry = await CreateInternalEntry(date, 1M);
        var volunteerEntry = await CreateVolunteerEntry(date, 1M);

        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = billableEntry.Date, Value = billableEntry.Value, TaskId = billableEntry.TaskId } });
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = internalEntry.Date, Value = internalEntry.Value, TaskId = internalEntry.TaskId } });
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = volunteerEntry.Date, Value = volunteerEntry.Value, TaskId = volunteerEntry.TaskId } });

        var overtime = (await _timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
            { FromDateInclusive = date, ToDateInclusive = date }))
            .OrderByDescending(x => x.CompensationRate)
            .ToList();

        Assert.Equal(3, overtime.Count);
        Assert.Equal(2M, overtime[0].Value);
        Assert.Equal(CompensationRates.BillableInvoiceModel, overtime[0].CompensationRate);
        Assert.Equal(1M, overtime[1].Value);
        Assert.Equal(CompensationRates.Internal, overtime[1].CompensationRate);
        Assert.Equal(1M, overtime[2].Value);
        Assert.Equal(CompensationRates.Volunteer, overtime[2].CompensationRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAvailableOvertime_InvoiceBasedUser_BillableOvertime_2HoursBefore2Point8HoursAfter()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var entry = await CreateBillableEntry(date, 9.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = entry.Date, Value = entry.Value, TaskId = entry.TaskId } });

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(2M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(2M * CompensationRates.BillableInvoiceModel, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAvailableOvertime_InvoiceBasedUser_InternalOvertime_2Point5HoursBefore2Point5HoursAfter()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var entry = await CreateInternalEntry(date, 10M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = entry.Date, Value = entry.Value, TaskId = entry.TaskId } });

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(2.5M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(2.5M * CompensationRates.Internal, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAvailableOvertime_InvoiceBasedUser_VolunteerOvertime_2Point5HoursBefore1Point25HoursAfter()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var entry = await CreateVolunteerEntry(date, 10M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = entry.Date, Value = entry.Value, TaskId = entry.TaskId } });

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(2.5M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(2.5M * CompensationRates.Volunteer, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAvailableOvertime_InvoiceBasedUser_ImposedOvertime_2HoursBefore4HoursAfter()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var volunteerEntry = await CreateVolunteerEntry(date, 7.5M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = volunteerEntry.Date, Value = volunteerEntry.Value, TaskId = volunteerEntry.TaskId } });

        var imposedEntry = await CreateImposedEntry(date, 2M);
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = imposedEntry.Date, Value = imposedEntry.Value, TaskId = imposedEntry.TaskId } });

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        Assert.Equal(2M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(2M * CompensationRates.Imposed, available.AvailableHoursAfterCompensation);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAvailableOvertime_InvoiceBasedUser_MixedTypes_4HoursBefore4Point3HoursAfter()
    {
        var date = new DateTime(2021, 12, 13); // Monday
        var billableEntry = await CreateBillableEntry(date, 9.5M);
        var internalEntry = await CreateInternalEntry(date, 1M);
        var volunteerEntry = await CreateVolunteerEntry(date, 1M);

        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = billableEntry.Date, Value = billableEntry.Value, TaskId = billableEntry.TaskId } });
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = internalEntry.Date, Value = internalEntry.Value, TaskId = internalEntry.TaskId } });
        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = volunteerEntry.Date, Value = volunteerEntry.Value, TaskId = volunteerEntry.TaskId } });

        var available = await _timeRegistrationService.GetAvailableOvertimeHoursNow();

        // 2h @1.4 + 1h @1.0 + 1h @0.5 = 4.3 after compensation
        Assert.Equal(4M, available.AvailableHoursBeforeCompensation);
        Assert.Equal(
            2M * CompensationRates.BillableInvoiceModel + 1M * CompensationRates.Internal + 1M * CompensationRates.Volunteer,
            available.AvailableHoursAfterCompensation);
    }

    private async Task<Hours> CreateBillableEntry(DateTime date, decimal hours)
    {
        var taskId = new Random().Next(1000, 10000000);
        var task = new Task { Id = taskId, Project = 1, CompensationType = CompensationType.Billable };
        await _context.Task.AddAsync(task);
        await _context.SaveChangesAsync();
        return new Hours { User = 1, Date = date, Value = hours, Task = task, TaskId = taskId };
    }

    private async Task<Hours> CreateInternalEntry(DateTime date, decimal hours)
    {
        var taskId = new Random().Next(1000, 10000000);
        var task = new Task { Id = taskId, Project = 1, CompensationType = CompensationType.Internal };
        await _context.Task.AddAsync(task);
        await _context.SaveChangesAsync();
        return new Hours { User = 1, Date = date, Value = hours, Task = task, TaskId = taskId };
    }

    private async Task<Hours> CreateVolunteerEntry(DateTime date, decimal hours)
    {
        var taskId = new Random().Next(1000, 10000000);
        var task = new Task { Id = taskId, Project = 1, CompensationType = CompensationType.Volunteer };
        await _context.Task.AddAsync(task);
        await _context.SaveChangesAsync();
        return new Hours { User = 1, Date = date, Value = hours, Task = task, TaskId = taskId };
    }

    private async Task<Hours> CreateImposedEntry(DateTime date, decimal hours)
    {
        var taskId = new Random().Next(1000, 10000000);
        var task = new Task { Id = taskId, Project = 1, CompensationType = CompensationType.Billable, Imposed = true };
        await _context.Task.AddAsync(task);
        await _context.SaveChangesAsync();
        return new Hours { User = 1, Date = date, Value = hours, Task = task, TaskId = taskId };
    }
}
