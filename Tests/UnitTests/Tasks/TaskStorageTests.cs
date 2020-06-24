using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.Tasks
{
    public class TaskStorageTests
    {
        [Fact]
        public void GetTasks_NoCriterias_AllTasks()
        {
            var context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new TaskStorage(context);

            var tasks = storage.GetTasks(new TaskQuerySearch(), 1);

            Assert.Equal(context.Task.Count(), tasks.Count());
        }

        [Fact]
        public void GetTasks_ProjectIsGiven_AllTasksWithSpecifiedProject()
        {
            var context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new TaskStorage(context);
            var tasks = storage.GetTasks(new TaskQuerySearch
            {
                Project = 1
            }, 1);

            Assert.True(1 == tasks.Single().Project.Id);
        }

        [Fact]
        public void GetTasks_CompensationRateIsGiven_AllTasksWithSpecifiedCompensationRate()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new TaskStorage(context);
            var tasks = storage.GetTasks(new TaskQuerySearch
            {
                CompensationRate = 1.0M
            }, 1);

            Assert.True(tasks.Single().CompensationRate == 1.0M);
        }

        [Fact]
        public void GetTasks_ProjectAndLockedIsGiven_AllTasksWithSpecifiedProjectAndLocked()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new TaskStorage(context);
            var tasks = storage.GetTasks(new TaskQuerySearch
            {
                Project = 2,
                Locked = true
            }, 1);

            Assert.True(tasks.Single().Project.Id == 2 && true == tasks.Single().Locked);
        }

        [Fact]
        public void FavoriteUpdater_UserCreatesNewFavorite_NewFavoriteIsCreated()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var previousNumberOfFavorites = context.TaskFavorites
                .Where(tf => tf.UserId == 1)
                .ToList().Count();

            var storage = new TaskStorage(context);
            var updater = new FavoriteUpdater(storage);

            updater.UpdateFavoriteTasks(new UpdateTasksDto
            {
                Id = 2,
                Favorite = true
            }, 1);

            var userFavorites = context.TaskFavorites
                .Where(tf => tf.UserId == 1)
                .ToList();

            Assert.Equal(previousNumberOfFavorites + 1, userFavorites.Count());
        }

        [Fact]
        public void FavoriteUpdater_UserCreatesNewFavoriteWithCompensationRate_NewFavoriteIsCreatedCompensationRateIsUnchanged()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var previousNumberOfFavorites = context.TaskFavorites
                .Where(tf => tf.UserId == 1)
                .ToList().Count();

            var storage = new TaskStorage(context);
            var updater = new FavoriteUpdater(storage);

            var previousCompensationRate = context.Task.FirstOrDefault(x => x.Id == 2).CompensationRate;

            updater.UpdateFavoriteTasks(new UpdateTasksDto
            {
                Id = 2,
                Favorite = true,
                CompensationRate = 2.5M
            }, 1);

            var task = context.Task.FirstOrDefault(x => x.Id == 2);

            Assert.Equal(previousCompensationRate, task.CompensationRate);
        }

        [Fact]
        public void FavoriteUpdater_UserRemovesExistingFavorite_ExistingFavoriteIsRemoved()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new TaskStorage(context);
            var updater = new FavoriteUpdater(storage);

            var previousNumberOfFavorites = context.TaskFavorites
                .Where(tf => tf.UserId == 1)
                .ToList().Count();

            updater.UpdateFavoriteTasks(new UpdateTasksDto
            {
                Id = 1,
                Favorite = false
            }, 1);

            var userFavorites = context.TaskFavorites
                .Where(tf => tf.UserId == 1)
                .ToList();

            Assert.Equal(previousNumberOfFavorites - 1, userFavorites.Count());
        }

        [Fact]
        public void TaskCreator_CreateNewTask_NewTaskIsCreated()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new TaskStorage(context);
            var creator = new TaskCreator(storage);

            var previousNumberOfTasks = context.Task.Count();

            creator.CreateTask(new CreateTaskDto
            {
                Name = "Prosjektleder",
                CompensationRate = 1.0M,
                Description = "",
                Locked = false,
                Project = 1
            }, 1);

            Assert.Equal(previousNumberOfTasks+1, context.Task.Count());
        }

        [Fact]
        public void TaskCreator_CreateNewTaskAlreadyExists_NoNewTaskIsCreated()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new TaskStorage(context);
            var creator = new TaskCreator(storage);

            var previousNumberOfTasks = context.Task.Count();

            creator.CreateTask(new CreateTaskDto
            {
                Name = "ExampleTask",
                CompensationRate = 1.0M,
                Description = "",
                Locked = false,
                Project = 1
            }, 1);

            Assert.Equal(previousNumberOfTasks, context.Task.Count());
        }

        [Fact]
        public void TaskCreator_UpdateOnlyCompensationRate_CompensationRateIsUpdated()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new TaskStorage(context);
            var creator = new TaskCreator(storage);

            creator.UpdateTask(new UpdateTasksDto
            {
                Id = 1,
                CompensationRate = 1.5M
            }, 1);

            var task = context.Task.FirstOrDefault(x => x.Id == 1);

            Assert.True(task.CompensationRate == 1.5M);
        }

        [Fact]
        public void TaskCreator_UpdateBothLockedAndName_LockedAndNameIsUpdated()
        {
            AlvTime_dbContext context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new TaskStorage(context);
            var creator = new TaskCreator(storage);

            creator.UpdateTask(new UpdateTasksDto
            {
                Id = 1,
                Locked = true,
                Name = "MyExampleTask"
            }, 1);

            var task = context.Task.FirstOrDefault(x => x.Id == 1);

            Assert.Equal("MyExampleTask", task.Name);
            Assert.True(task.Locked == true);
        }
    }
}
