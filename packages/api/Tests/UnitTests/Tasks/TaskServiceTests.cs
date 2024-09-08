using System;
using System.Collections.Generic;
using AlvTime.Business.Tasks;
using System.Linq;
using AlvTime.Business.Options;
using AlvTime.Business.TimeRegistration;
using AlvTime.Persistence.DatabaseModels;
using Moq;
using Xunit;
using Task = System.Threading.Tasks.Task;
using User = AlvTime.Business.Users.User;
using AlvTime.Business.Users;
using AlvTime.Business.Utils;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;

namespace Tests.UnitTests.Tasks;

public class TaskServiceTests
{
    private readonly AlvTime_dbContext _context;
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly TimeRegistrationService _timeRegistrationService;

    public TaskServiceTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .WithTasks()
            .WithTaskFavorites()
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

        var user = new User
        {
            Id = 1,
            Email = "someone@alv.no",
            Name = "Someone"
        };

        _timeRegistrationService = CreateTimeRegistrationService();

        _userContextMock.Setup(context => context.GetCurrentUser())
            .Returns(Task.FromResult(user));
    }

    [Fact]
    public async Task GetTasks_NoCriterias_AllTasks()
    {
        var taskService = CreateTaskService(_context);

        var results = await taskService.GetTasksForUser(new TaskQuerySearch());

        Assert.Equal(_context.Task.Count(), results.Value.Count());
    }

    [Fact]
    public async Task GetTasks_ProjectIsGiven_AllTasksWithSpecifiedProject()
    {
        var taskService = CreateTaskService(_context);

        var results = await taskService.GetTasksForUser(new TaskQuerySearch
        {
            Project = 1
        });

        Assert.True(1 == results.Value.Single().Project.Id);
    }

    [Fact]
    public async Task GetTasks_ProjectAndLockedIsGiven_AllTasksWithSpecifiedProjectAndLocked()
    {
        var taskService = CreateTaskService(_context);
        var results = await taskService.GetTasksForUser(new TaskQuerySearch
        {
            Project = 2,
            Locked = true
        });

        Assert.True(results.Value.Single().Project.Id == 2 && results.Value.Single().Locked);
    }

    [Fact]
    public async Task FavoriteUpdater_UserCreatesNewFavorite_NewFavoriteIsCreated()
    {
        var previousNumberOfFavorites = _context.TaskFavorites
            .Where(tf => tf.UserId == 1)
            .ToList().Count();

        var taskService = CreateTaskService(_context);

        await taskService.UpdateFavoriteTasks(new List<(int id, bool favorite)> { (2, true) });

        var userFavorites = _context.TaskFavorites
            .Where(tf => tf.UserId == 1)
            .ToList();

        Assert.Equal(previousNumberOfFavorites + 1, userFavorites.Count());
    }

    [Fact]
    public async Task
        FavoriteUpdater_UserCreatesNewFavoriteWithCompensationRate_NewFavoriteIsCreatedCompensationRateIsUnchanged()
    {
        var taskService = CreateTaskService(_context);

        var previousCompensationRate = _context.Task.FirstOrDefault(x => x.Id == 2)?.CompensationRate;

        await taskService.UpdateFavoriteTasks(new List<(int id, bool favorite)> { (2, true) });

        var task = _context.Task.FirstOrDefault(x => x.Id == 2);

        Assert.Equal(previousCompensationRate, task?.CompensationRate);
    }

    [Fact]
    public async Task FavoriteUpdater_UserRemovesExistingFavorite_ExistingFavoriteIsRemoved()
    {
        var taskService = CreateTaskService(_context);

        var previousNumberOfFavorites = _context.TaskFavorites
            .Where(tf => tf.UserId == 1)
            .ToList().Count;

        await taskService.UpdateFavoriteTasks(new List<(int id, bool favorite)> { (1, false) });

        var userFavorites = _context.TaskFavorites
            .Where(tf => tf.UserId == 1)
            .ToList();

        Assert.Equal(previousNumberOfFavorites - 1, userFavorites.Count);
    }

    [Fact]
    public async Task TaskService_CreateNewTask_NewTaskIsCreated()
    {
        var taskService = CreateTaskService(_context);

        var previousNumberOfTasks = _context.Task.Count();

        await taskService.CreateTask(new TaskDto
        {
            Name = "Prosjektleder", Description = "", Locked = false
        }, 1);

        Assert.Equal(previousNumberOfTasks + 1, _context.Task.Count());
    }

    [Fact]
    public async Task TaskService_CreateNewTaskAlreadyExists_NoNewTaskIsCreated()
    {
        var taskService = CreateTaskService(_context);

        var previousNumberOfTasks = _context.Task.Count();

        var taskResult = await taskService.CreateTask(new TaskDto
        {
            Name = "ExampleTask", Description = "", Locked = false
        }, 1);
        Assert.False(taskResult.IsSuccess);
        Assert.Single(taskResult.Errors);
        Assert.Equal(previousNumberOfTasks, _context.Task.Count());
    }

    [Fact]
    public async Task TaskService_UpdateBothLockedAndName_LockedAndNameIsUpdated()
    {
        var taskService = CreateTaskService(_context);

        await taskService.UpdateTask(new TaskDto
            { Id = 1, Locked = true, Name = "MyExampleTask", CompensationRate = 1.50M });

        var task = _context.Task.FirstOrDefault(x => x.Id == 1);

        Assert.Equal("MyExampleTask", task?.Name);
        Assert.True(task.Locked);
    }

    [Fact]
    public async Task ReturnLatestTasksWhenUserHasSubmittedTimeEntryOnTask()
    {
        var taskService = CreateTaskService(_context);

        var tasks = await taskService.GetTasksForUser(new TaskQuerySearch());

        var timeEntryDto = new CreateTimeEntryDto
        {
            Date = DateTime.Today,
            Value = 7,
            TaskId = tasks.Value.First().Id
        };

        var latestTasksForUserBeforeUpdate = await taskService.GetLatestTasksForUser();
        Assert.Empty(latestTasksForUserBeforeUpdate);

        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto> { timeEntryDto });

        var latestTasksForUser = await taskService.GetLatestTasksForUser();
        Assert.Single(latestTasksForUser);
    }

    [Fact]
    public async Task OnlyReturnUpTo5LatestTasks()
    {
        var taskService = CreateTaskService(_context);

        var taskResponseDtos = (await taskService.GetTasksForUser(new TaskQuerySearch())).Value
            .Where(dto => !dto.Locked).ToList();
        foreach (var taskResponseDto in taskResponseDtos)
        {
            var timeEntryDto = new CreateTimeEntryDto
            {
                Date = DateTime.Today,
                Value = 7,
                TaskId = taskResponseDto.Id
            };
            await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto> { timeEntryDto });
        }

        Assert.Equal(10, taskResponseDtos.Count);

        var latestTasksForUser = await taskService.GetLatestTasksForUser();
        Assert.Equal(5, latestTasksForUser.ToList().Count);
    }

    [Fact]
    public async Task CheckOnlyTheLatest30DaysWhenFindingLastTasks()
    {
        var taskService = CreateTaskService(_context);

        var tasks = await taskService.GetTasksForUser(new TaskQuerySearch());

        var timeEntryDto = new CreateTimeEntryDto
        {
            Date = DateTime.Today.AddDays(-31),
            Value = 7,
            TaskId = tasks.Value.First().Id
        };

        var latestTasksForUserBeforeUpdate = await taskService.GetLatestTasksForUser();
        Assert.Empty(latestTasksForUserBeforeUpdate);

        await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto> { timeEntryDto });

        var latestTasksForUser = await taskService.GetLatestTasksForUser();
        Assert.Empty(latestTasksForUser);
    }

    private TaskService CreateTaskService(AlvTime_dbContext dbContext)
    {
        return new TaskService(_timeRegistrationService, new TaskStorage(dbContext), _userContextMock.Object);
    }

    private TimeRegistrationService CreateTimeRegistrationService()
    {
        return new TimeRegistrationService(_options, _userContextMock.Object, CreateTaskUtils(),
            new TimeRegistrationStorage(_context), new DbContextScope(_context),
            new PayoutStorage(_context, new DateAlvTime()),
            new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)));
    }

    private TaskUtils CreateTaskUtils()
    {
        return new TaskUtils(new TaskStorage(_context), _options);
    }
}