using System;
using System.Collections.Generic;
using AlvTime.Business.Tasks;
using AlvTime.Persistence.Repositories;
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
using Microsoft.Extensions.Options;

namespace Tests.UnitTests.Tasks
{
    public class LatestTaskStorageTests
    {
        private readonly AlvTime_dbContext _context;
        private readonly IOptionsMonitor<TimeEntryOptions> _options;
        private readonly Mock<IUserContext> _userContextMock;
        private readonly TimeRegistrationService _timeRegistrationService;
        private readonly LatestTaskService _latestTaskService;

        public LatestTaskStorageTests()
        {
            _context = new AlvTimeDbContextBuilder()
                .WithTasks()
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
            _latestTaskService = CreateLatestTaskService(_timeRegistrationService);

            _userContextMock.Setup(context => context.GetCurrentUser())
                .Returns(Task.FromResult(user));
        }


        private TimeRegistrationService CreateTimeRegistrationService()
        {
            return new TimeRegistrationService(_options, _userContextMock.Object, CreateTaskUtils(),
                new TimeRegistrationStorage(_context), new DbContextScope(_context),
                new PayoutStorage(_context, new DateAlvTime()),
                new UserService(new UserRepository(_context), new TimeRegistrationStorage(_context)));
        }

        private LatestTaskService CreateLatestTaskService(TimeRegistrationService timeRegistrationService)
        {
            return new LatestTaskService(CreateTaskService(_context), timeRegistrationService);
        }

        private TaskUtils CreateTaskUtils()
        {
            return new TaskUtils(new TaskStorage(_context), _options);
        }

        [Fact]
        public async Task ReturnLatestTasksWhenUserHasSubmittedTimeEntryOnTask()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();

            var taskService = CreateTaskService(context);

            var tasks = await taskService.GetTasksForUser(new TaskQuerySearch());

            var timeEntryDto = new CreateTimeEntryDto
            {
                Date = DateTime.Today,
                Value = 7,
                TaskId = tasks.First().Id
            };

            var latestTasksForUserBeforeUpdate = await _latestTaskService.GetLatestTasksForUser();
            Assert.Empty(latestTasksForUserBeforeUpdate);

            await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto> { timeEntryDto });

            var latestTasksForUser = await _latestTaskService.GetLatestTasksForUser();
            Assert.Single(latestTasksForUser);
        }

        [Fact]
        public async Task OnlyReturnUpTo5LatestTasks()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();

            var taskService = CreateTaskService(context);

            var taskResponseDtos = (await taskService.GetTasksForUser(new TaskQuerySearch())).ToList().Where(dto => !dto.Locked).ToList();
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

            Assert.Equal(6,taskResponseDtos.Count());

            var latestTasksForUser = await _latestTaskService.GetLatestTasksForUser();
            Assert.Equal(5, latestTasksForUser.ToList().Count);
        }

        [Fact]
        public async Task CheckOnlyTheLatest30DaysWhenFindingLastTasks()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();

            var taskService = CreateTaskService(context);

            var tasks = await taskService.GetTasksForUser(new TaskQuerySearch());

            var timeEntryDto = new CreateTimeEntryDto
            {
                Date = DateTime.Today.AddDays(-31),
                Value = 7,
                TaskId = tasks.First().Id
            };

            var latestTasksForUserBeforeUpdate = await _latestTaskService.GetLatestTasksForUser();
            Assert.Empty(latestTasksForUserBeforeUpdate);

            await _timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto> { timeEntryDto });

            var latestTasksForUser = await _latestTaskService.GetLatestTasksForUser();
            Assert.Empty(latestTasksForUser);
        }

        private static TaskService CreateTaskService(AlvTime_dbContext dbContext)
        {
            var mockUserContext = new Mock<IUserContext>();

            var user = new User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone"
            };

            mockUserContext.Setup(context => context.GetCurrentUser()).Returns(Task.FromResult(user));
            return new TaskService(new TaskStorage(dbContext), mockUserContext.Object);
        }
    }
}