using AlvTimeApi.Controllers.Tasks;
using AlvTimeApi.Dto;
using AlvTimeWebApi2.DataBaseModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.UnitTests
{
    public class TasksControllerUT
    {
        [Fact]
        public void UpdateFavoriteTasks_InputTrueCurrentTrue_NoChange()
        {
            var options = new DbContextOptionsBuilder<AlvTimeDBContext>()
                            .UseInMemoryDatabase(databaseName: "Update_favorites")
                            .Options;

            using (var context = new AlvTimeDBContext(options))
            {
                var service = new TasksController(context);

                var favorite = new TaskFavorites
                {
                    TaskId = 1,
                    UserId = 1
                };

                var task = new Task
                {
                    Description = "Test",
                    Favorite = true,
                    HourRate = 1000,
                    Id = 1,
                    Locked = false,
                    Name = "Utvikler",
                    Project = 3
                };

                var user = new User
                {
                    Email = "ansatten@alv.no",
                    Id = 1,
                    Name = "Ansatt En"
                };

                context.Add(task);
                context.Add(favorite);
                context.Add(user);
                context.SaveChanges();

                var list = new List<UpdateTasksDto>();

                var taskDto = new UpdateTasksDto
                {
                    Favorite = true,
                    Id = 1
                };
                list.Add(taskDto);

                service.UpdateFavoriteTasks(list);

                var updatedTask = context.Task.FirstOrDefault();
                var updatedFavorite = context.TaskFavorites.FirstOrDefault();

                Assert.True(updatedTask.Favorite);
                Assert.NotNull(updatedFavorite);
            }
        }

        [Fact]
        public void UpdateFavoriteTasks_InputTrueCurrentFalse_FavoriteTrue()
        {
            var options = new DbContextOptionsBuilder<AlvTimeDBContext>()
                            .UseInMemoryDatabase(databaseName: "Update_favorites")
                            .Options;

            using (var context = new AlvTimeDBContext(options))
            {
                var service = new TasksController(context);

                var task = new Task
                {
                    Description = "Test",
                    Favorite = false,
                    HourRate = 1000,
                    Id = 1,
                    Locked = false,
                    Name = "Utvikler",
                    Project = 3
                };

                var user = new User
                {
                    Email = "ansatten@alv.no",
                    Id = 1,
                    Name = "Ansatt En"
                };

                context.Add(task);
                context.Add(user);
                context.SaveChanges();

                var list = new List<UpdateTasksDto>();

                var taskDto = new UpdateTasksDto
                {
                    Favorite = true,
                    Id = 1
                };
                list.Add(taskDto);

                service.UpdateFavoriteTasks(list);

                var updatedTask = context.Task.FirstOrDefault();
                var updatedFavorite = context.TaskFavorites.FirstOrDefault();

                Assert.True(updatedTask.Favorite);
                Assert.NotNull(updatedFavorite);
            }
        }

        [Fact]
        public void UpdateFavoriteTasks_InputFalseCurrentFalse_NoChange()
        {
            var options = new DbContextOptionsBuilder<AlvTimeDBContext>()
                            .UseInMemoryDatabase(databaseName: "Update_favorites")
                            .Options;

            using (var context = new AlvTimeDBContext(options))
            {
                var service = new TasksController(context);

                var task = new Task
                {
                    Description = "Test",
                    Favorite = false,
                    HourRate = 1000,
                    Id = 1,
                    Locked = false,
                    Name = "Utvikler",
                    Project = 3
                };

                var user = new User
                {
                    Email = "ansatten@alv.no",
                    Id = 1,
                    Name = "Ansatt En"
                };

                context.Add(task);
                context.Add(user);
                context.SaveChanges();

                var list = new List<UpdateTasksDto>();

                var taskDto = new UpdateTasksDto
                {
                    Favorite = false,
                    Id = 1
                };
                list.Add(taskDto);

                service.UpdateFavoriteTasks(list);

                var updatedTask = context.Task.FirstOrDefault();
                var updatedFavorite = context.TaskFavorites.FirstOrDefault();

                Assert.False(updatedTask.Favorite);
                Assert.Null(updatedFavorite);
            }
        }

        [Fact]
        public void UpdateFavoriteTasks_InputFalseCurrentTrue_FavoriteFalse()
        {
            var options = new DbContextOptionsBuilder<AlvTimeDBContext>()
                            .UseInMemoryDatabase(databaseName: "Update_favorites")
                            .Options;

            using (var context = new AlvTimeDBContext(options))
            {
                var service = new TasksController(context);

                var favorite = new TaskFavorites
                {
                    TaskId = 1,
                    UserId = 1
                };

                var task = new Task
                {
                    Description = "Test",
                    Favorite = true,
                    HourRate = 1000,
                    Id = 1,
                    Locked = false,
                    Name = "Utvikler",
                    Project = 3
                };

                var user = new User
                {
                    Email = "ansatten@alv.no",
                    Id = 1,
                    Name = "Ansatt En"
                };

                context.Add(task);
                context.Add(favorite);
                context.Add(user);
                context.SaveChanges();

                var list = new List<UpdateTasksDto>();

                var taskDto = new UpdateTasksDto
                {
                    Favorite = false,
                    Id = 1
                };
                list.Add(taskDto);

                service.UpdateFavoriteTasks(list);

                var updatedTask = context.Task.FirstOrDefault();
                var updatedFavorite = context.TaskFavorites.FirstOrDefault();

                Assert.False(updatedTask.Favorite);
                Assert.Null(updatedFavorite);
            }
        }

        [Fact]
        public void UpdateFavoriteTasks_TaskDoesNotExist_ExceptionThrown()
        {
            var options = new DbContextOptionsBuilder<AlvTimeDBContext>()
                            .UseInMemoryDatabase(databaseName: "Update_favorites")
                            .Options;

            using (var context = new AlvTimeDBContext(options))
            {
                var service = new TasksController(context);

                var user = new User
                {
                    Email = "ansatten@alv.no",
                    Id = 1,
                    Name = "Ansatt En"
                };

                context.Add(user);
                context.SaveChanges();

                var list = new List<UpdateTasksDto>();

                var taskDto = new UpdateTasksDto
                {
                    Favorite = false,
                    Id = 1
                };
                list.Add(taskDto);

                service.UpdateFavoriteTasks(list);

                var updatedTask = context.Task.FirstOrDefault();
                var updatedFavorite = context.TaskFavorites.FirstOrDefault();

                Assert.Null(updatedTask);
                Assert.Null(updatedFavorite);
            }
        }
    }
}
