using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business;
using AlvTime.Business.Absence;
using AlvTime.Business.Options;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using AlvTime.Business.Utils;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.UnitTests.Absence;

public class AbsencePlanServiceTests
{
    private const int UnpaidHolidayTask = 12;
    private const int PaidHolidayTask = 13;
    private const int SickDaysTask = 14;
    private const int LockedAbsenceTask = 15;
    private const int FlexTask = 18;
    private const int ProjectTask = 1;

    private readonly AlvTime_dbContext _context;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private readonly Mock<IUserContext> _userContextMock;

    public AbsencePlanServiceTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .WithTasks()
            .WithLeaveTasks()
            .WithProjects()
            .WithStaticSalaryUsers()
            .WithCustomers()
            .CreateDbContext();

        _context.Task.Add(new AlvTime.Persistence.DatabaseModels.Task
        {
            Id = LockedAbsenceTask,
            Description = "",
            Project = 9,
            Name = "LockedAbsence",
            Locked = true
        });
        _context.SaveChanges();

        var entryOptions = new TimeEntryOptions
        {
            SickDaysTask = SickDaysTask,
            PaidHolidayTask = PaidHolidayTask,
            UnpaidHolidayTask = UnpaidHolidayTask,
            FlexTask = FlexTask,
            StartOfOvertimeSystem = new DateTime(2020, 01, 01),
            AbsenceProject = 9,
            AbsencePlanTasks = [UnpaidHolidayTask, SickDaysTask, PaidHolidayTask, LockedAbsenceTask]
        };
        _options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);

        _userContextMock = new Mock<IUserContext>();
        _userContextMock.Setup(context => context.GetCurrentUser()).ReturnsAsync(new AlvTime.Business.Users.User
        {
            Id = 1,
            Email = "someone@alv.no",
            Name = "Someone",
            Oid = "12345678-1234-1234-1234-123456789012"
        });
    }

    // 1.1 Koder

    [Fact]
    public async Task GetAbsencePlanTasks_ReturnsConfiguredUnlockedTasks_WithPaidHolidayFirst()
    {
        var tasks = await CreateService().GetAbsencePlanTasks();

        Assert.Equal(PaidHolidayTask, tasks.First().Id);
        Assert.Equal(new[] { UnpaidHolidayTask, PaidHolidayTask, SickDaysTask }, tasks.Select(t => t.Id).OrderBy(id => id));
        Assert.DoesNotContain(tasks, t => t.Id == FlexTask);
        Assert.DoesNotContain(tasks, t => t.Id == LockedAbsenceTask);
    }

    [Fact]
    public async Task GetAbsencePlanTasks_OnlyPaidHolidayIsMarkedAsPaidHoliday()
    {
        var tasks = await CreateService().GetAbsencePlanTasks();

        Assert.True(tasks.Single(t => t.Id == PaidHolidayTask).IsPaidHoliday);
        Assert.All(tasks.Where(t => t.Id != PaidHolidayTask), t => Assert.False(t.IsPaidHoliday));
    }

    // 1.2 Timer per dag

    [Fact]
    public async Task GetPlan_Weekdays_RegisterFullDay()
    {
        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 07))); // Mon-Fri

        Assert.True(result.IsSuccess);
        Assert.All(result.Value.Days, d =>
        {
            Assert.Equal(AbsencePlanDayStatus.Register, d.Status);
            Assert.Equal(7.5M, d.Hours);
        });
        Assert.Equal(5, result.Value.TotalDays);
        Assert.Equal(37.5M, result.Value.TotalHours);
    }

    [Fact]
    public async Task GetPlan_UserEmployed80Percent_Registers6Hours()
    {
        AddEmploymentRate(0.8M, new DateTime(2022, 01, 01), new DateTime(2022, 12, 31));

        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 03)));

        Assert.Equal(6M, result.Value.Days.Single().Hours);
    }

    [Fact]
    public async Task GetPlan_Percentage50_RegistersHalfDay()
    {
        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 03), percentage: 50));

        Assert.Equal(3.75M, result.Value.Days.Single().Hours);
    }

    // 1.3 Helg og røde dager

    [Fact]
    public async Task GetPlan_WholeWeek_WeekendIsSkipped()
    {
        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 09))); // Mon-Sun

        Assert.Equal(7, result.Value.Days.Count);
        Assert.Equal(AbsencePlanDayStatus.Weekend, DayOn(result.Value, new DateTime(2022, 01, 08)).Status);
        Assert.Equal(AbsencePlanDayStatus.Weekend, DayOn(result.Value, new DateTime(2022, 01, 09)).Status);
        Assert.Equal(0M, DayOn(result.Value, new DateTime(2022, 01, 08)).Hours);
        Assert.Equal(5, result.Value.TotalDays);
    }

    [Theory]
    [InlineData(PaidHolidayTask)]
    [InlineData(SickDaysTask)]
    public async Task GetPlan_RedDays_AreSkipped(int taskId)
    {
        var result = await CreateService().GetPlan(Request(new DateTime(2022, 05, 16), new DateTime(2022, 05, 27), taskId));

        Assert.Equal(AbsencePlanDayStatus.RedDay, DayOn(result.Value, new DateTime(2022, 05, 17)).Status); // Grunnlovsdag
        Assert.Equal(AbsencePlanDayStatus.RedDay, DayOn(result.Value, new DateTime(2022, 05, 26)).Status); // Kristi himmelfart
        Assert.Equal(AbsencePlanDayStatus.Register, DayOn(result.Value, new DateTime(2022, 05, 16)).Status);
        Assert.Equal(8, result.Value.TotalDays);
    }

    // 1.4 Allerede registrert

    [Fact]
    public async Task GetPlan_FullDayAlreadyRegisteredOnTask_AlreadyRegistered()
    {
        await RegisterHours(new DateTime(2022, 01, 03), 7.5M, PaidHolidayTask);

        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 04)));

        Assert.Equal(AbsencePlanDayStatus.AlreadyRegistered, DayOn(result.Value, new DateTime(2022, 01, 03)).Status);
        Assert.Equal(AbsencePlanDayStatus.Register, DayOn(result.Value, new DateTime(2022, 01, 04)).Status);
        Assert.Equal(1, result.Value.TotalDays);
    }

    [Fact]
    public async Task GetPlan_PartialDayAlreadyRegisteredOnTask_RegisterUpdatesToFullDay()
    {
        await RegisterHours(new DateTime(2022, 01, 03), 3.75M, PaidHolidayTask);

        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 03)));

        var day = result.Value.Days.Single();
        Assert.Equal(AbsencePlanDayStatus.Register, day.Status);
        Assert.Equal(7.5M, day.Hours);
    }

    // 1.5 Timer på andre koder

    [Fact]
    public async Task GetPlan_OtherHoursAndNotFillPartialDays_HasOtherHours()
    {
        await RegisterHours(new DateTime(2022, 01, 04), 3M, ProjectTask);

        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 05)));

        var day = DayOn(result.Value, new DateTime(2022, 01, 04));
        Assert.Equal(AbsencePlanDayStatus.HasOtherHours, day.Status);
        Assert.Equal(3M, day.ExistingOtherHours);
        Assert.Equal(0M, day.Hours);
        Assert.Equal(2, result.Value.TotalDays);
    }

    [Fact]
    public async Task GetPlan_OtherHoursAndFillPartialDays_RegistersRestOfDay()
    {
        await RegisterHours(new DateTime(2022, 01, 04), 3M, ProjectTask);

        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 04), new DateTime(2022, 01, 04), fillPartialDays: true));

        var day = result.Value.Days.Single();
        Assert.Equal(AbsencePlanDayStatus.Register, day.Status);
        Assert.Equal(4.5M, day.Hours);
        Assert.Equal(3M, day.ExistingOtherHours);
    }

    [Fact]
    public async Task GetPlan_OtherHoursAndFillPartialDaysWithPercentage_RegistersAtMostPercentage()
    {
        await RegisterHours(new DateTime(2022, 01, 04), 3M, ProjectTask);

        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 04), new DateTime(2022, 01, 04), percentage: 50, fillPartialDays: true));

        Assert.Equal(3.75M, result.Value.Days.Single().Hours);
    }

    [Fact]
    public async Task GetPlan_FullDayOtherHoursAndFillPartialDays_HasOtherHours()
    {
        await RegisterHours(new DateTime(2022, 01, 04), 7.5M, ProjectTask);

        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 04), new DateTime(2022, 01, 04), fillPartialDays: true));

        Assert.Equal(AbsencePlanDayStatus.HasOtherHours, result.Value.Days.Single().Status);
    }

    // 1.6 Sperrer bakover i tid

    [Fact]
    public async Task GetPlan_CustomerLockedTo_LockedDaysAreBlockedWithReason()
    {
        _context.Customer.First(c => c.Id == 1).LockedTo = new DateTime(2022, 01, 05);
        await _context.SaveChangesAsync();

        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 07)));

        var blocked = DayOn(result.Value, new DateTime(2022, 01, 05));
        Assert.Equal(AbsencePlanDayStatus.Blocked, blocked.Status);
        Assert.False(string.IsNullOrWhiteSpace(blocked.Reason));
        Assert.Equal(AbsencePlanDayStatus.Register, DayOn(result.Value, new DateTime(2022, 01, 06)).Status);
        Assert.Equal(2, result.Value.TotalDays);
    }

    // 1.7 Ugyldig forespørsel

    [Theory]
    [InlineData(FlexTask)]
    [InlineData(ProjectTask)]
    [InlineData(LockedAbsenceTask)]
    public async Task GetPlan_TaskNotAllowed_Fails(int taskId)
    {
        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 07), taskId));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetPlan_FromAfterTo_Fails()
    {
        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 07), new DateTime(2022, 01, 03)));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetPlan_PeriodLongerThanOneYear_Fails()
    {
        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2023, 01, 04)));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetPlan_PeriodOfExactlyOneYear_Succeeds()
    {
        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2023, 01, 02)));

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(101)]
    public async Task GetPlan_InvalidPercentage_Fails(decimal percentage)
    {
        var result = await CreateService().GetPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 07), percentage: percentage));

        Assert.False(result.IsSuccess);
    }

    // 1.8 Lagring

    [Fact]
    public async Task RegisterPlan_SavesOnlyRegisterDays()
    {
        await RegisterHours(new DateTime(2022, 01, 04), 3M, ProjectTask);

        var request = Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 09));
        request.Comment = "Juleferie";
        var result = await CreateService().RegisterPlan(request);

        Assert.True(result.IsSuccess);
        var saved = _context.Hours.Where(h => h.TaskId == PaidHolidayTask).OrderBy(h => h.Date).ToList();
        Assert.Equal(
            new[] { new DateTime(2022, 01, 03), new DateTime(2022, 01, 05), new DateTime(2022, 01, 06), new DateTime(2022, 01, 07) },
            saved.Select(h => h.Date.Date));
        Assert.All(saved, h =>
        {
            Assert.Equal(7.5M, h.Value);
            Assert.Equal("Juleferie", h.Comment);
        });
        Assert.Equal(4, result.Value.TotalDays);
    }

    [Fact]
    public async Task RegisterPlan_PartialDayAlreadyRegistered_UpdatesToFullDay()
    {
        await RegisterHours(new DateTime(2022, 01, 03), 3.75M, PaidHolidayTask);

        await CreateService().RegisterPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 03)));

        Assert.Equal(7.5M, _context.Hours.Single(h => h.TaskId == PaidHolidayTask).Value);
    }

    [Fact]
    public async Task RegisterPlan_InvalidRequest_SavesNothing()
    {
        var result = await CreateService().RegisterPlan(Request(new DateTime(2022, 01, 03), new DateTime(2022, 01, 07), FlexTask));

        Assert.False(result.IsSuccess);
        Assert.Empty(_context.Hours);
    }

    private static AbsencePlanRequest Request(DateTime from, DateTime to, int taskId = PaidHolidayTask,
        decimal percentage = 100, bool fillPartialDays = false) => new()
    {
        TaskId = taskId,
        FromDateInclusive = from,
        ToDateInclusive = to,
        Percentage = percentage,
        FillPartialDays = fillPartialDays
    };

    private static AbsencePlanDayDto DayOn(AbsencePlanDto plan, DateTime date) =>
        plan.Days.Single(d => d.Date.Date == date.Date);

    private void AddEmploymentRate(decimal rate, DateTime from, DateTime to)
    {
        _context.EmploymentRate.Add(new EmploymentRate { UserId = 1, Rate = rate, FromDate = from, ToDate = to });
        _context.SaveChanges();
    }

    private async Task RegisterHours(DateTime date, decimal value, int taskId)
    {
        var result = await CreateTimeRegistrationService().UpsertTimeEntry(new List<CreateTimeEntryDto>
            { new() { Date = date, Value = value, TaskId = taskId } });
        Assert.True(result.IsSuccess);
    }

    private AbsencePlanService CreateService()
    {
        var taskStorage = new TaskStorage(_context);
        var timeRegistrationStorage = new TimeRegistrationStorage(_context);
        return new AbsencePlanService(_options, _userContextMock.Object, taskStorage, timeRegistrationStorage,
            CreateTimeRegistrationService(), CreateUserService());
    }

    private TimeRegistrationService CreateTimeRegistrationService() =>
        new(_options, _userContextMock.Object, new TaskUtils(new TaskStorage(_context), _options),
            new TimeRegistrationStorage(_context), new DbContextScope(_context),
            new PayoutStorage(_context, new DateAlvTime()), CreateUserService());

    private UserService CreateUserService() =>
        new(new UserRepository(_context), new TimeRegistrationStorage(_context));
}
