using System.Collections.Generic;
using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTime.Persistence.Repositories;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Persistence.DataBaseModels;
using Moq;
using Xunit;
using User = AlvTime.Business.Models.User;

namespace Tests.UnitTests.Tasks
{
    public class TaskStorageTests
    {
        [Fact]
        public void GetTasks_NoCriterias_AllTasks()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();

            var taskService = CreateTaskService(context);

            var tasks = taskService.GetTasksForUser(new TaskQuerySearch());

            Assert.Equal(context.Task.Count(), tasks.Count());
        }

        [Fact]
        public void GetTasks_ProjectIsGiven_AllTasksWithSpecifiedProject()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();
            
            var taskService = CreateTaskService(context);

            var tasks = taskService.GetTasksForUser(new TaskQuerySearch
            {
                Project = 1
            });

            Assert.True(1 == tasks.Single().Project.Id);
        }

        [Fact]
        public void GetTasks_ProjectAndLockedIsGiven_AllTasksWithSpecifiedProjectAndLocked()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();

            var taskService = CreateTaskService(context);
            var tasks = taskService.GetTasksForUser(new TaskQuerySearch
            {
                Project = 2,
                Locked = true
            });

            Assert.True(tasks.Single().Project.Id == 2 && true == tasks.Single().Locked);
        }

        [Fact]
        public void FavoriteUpdater_UserCreatesNewFavorite_NewFavoriteIsCreated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();

            var previousNumberOfFavorites = context.TaskFavorites
                .Where(tf => tf.UserId == 1)
                .ToList().Count();
            
            var taskService = CreateTaskService(context);

            taskService.UpdateFavoriteTasks(new List<(int id, bool favorite)> { (2, true) });

            var userFavorites = context.TaskFavorites
                .Where(tf => tf.UserId == 1)
                .ToList();

            Assert.Equal(previousNumberOfFavorites + 1, userFavorites.Count());
        }

        [Fact]
        public void FavoriteUpdater_UserCreatesNewFavoriteWithCompensationRate_NewFavoriteIsCreatedCompensationRateIsUnchanged()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();

            var taskService = CreateTaskService(context);

            var previousCompensationRate = context.Task.FirstOrDefault(x => x.Id == 2).CompensationRate;

            taskService.UpdateFavoriteTasks(new List<(int id, bool favorite)> { (2, true) });

            var task = context.Task.FirstOrDefault(x => x.Id == 2);

            Assert.Equal(previousCompensationRate, task.CompensationRate);
        }

        [Fact]
        public void FavoriteUpdater_UserRemovesExistingFavorite_ExistingFavoriteIsRemoved()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .WithTaskFavorites()
                .CreateDbContext();
            
            var taskService = CreateTaskService(context);

            var previousNumberOfFavorites = context.TaskFavorites
                .Where(tf => tf.UserId == 1)
                .ToList().Count();
            
            taskService.UpdateFavoriteTasks(new List<(int id, bool favorite)> { (1, false) });

            var userFavorites = context.TaskFavorites
                .Where(tf => tf.UserId == 1)
                .ToList();

            Assert.Equal(previousNumberOfFavorites - 1, userFavorites.Count());
        }

        [Fact]
        public void TaskCreator_CreateNewTask_NewTaskIsCreated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();
            
            var taskService = CreateTaskService(context);

            var previousNumberOfTasks = context.Task.Count();

            taskService.CreateTasks(new List<CreateTaskDto>
            {
                new("Prosjektleder", "", Locked: false, Project: 1)
            });

            Assert.Equal(previousNumberOfTasks+1, context.Task.Count());
        }

        [Fact]
        public void TaskCreator_CreateNewTaskAlreadyExists_NoNewTaskIsCreated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();
            
            var taskService = CreateTaskService(context);

            var previousNumberOfTasks = context.Task.Count();
            
            taskService.CreateTasks(new List<CreateTaskDto>
            {
                new("ExampleTask", "", Locked: false, Project: 1)
            });

            Assert.Equal(previousNumberOfTasks, context.Task.Count());
        }

        [Fact]
        public void TaskCreator_UpdateBothLockedAndName_LockedAndNameIsUpdated()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithProjects()
                .WithCustomers()
                .CreateDbContext();
            
            var taskService = CreateTaskService(context);

            taskService.UpdateTasks(new List<UpdateTaskDto> { new(1, true, "MyExampleTask", null) });

            var task = context.Task.FirstOrDefault(x => x.Id == 1);

            Assert.Equal("MyExampleTask", task.Name);
            Assert.True(task.Locked == true);
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

            mockUserContext.Setup(context => context.GetCurrentUser()).Returns(user);
            
            return new TaskService(new TaskStorage(dbContext), mockUserContext.Object);
        }
    }
}
