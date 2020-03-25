using AlvTimeWebApi.DatabaseModels;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Tasks
{
    [Route("api/user")]
    [ApiController]
    public class TasksController : Controller
    {
        private readonly AlvTime_dbContext _database;
        private CreatedObjectReturner returnObjects;
        private ExistingObjectFinder checkExisting;

        public TasksController(AlvTime_dbContext database)
        {
            _database = database;
            returnObjects = new CreatedObjectReturner(_database);
            checkExisting = new ExistingObjectFinder(_database);
        }

        [HttpGet("Tasks")]
        [Authorize]
        public ActionResult<IEnumerable<TaskResponseDto>> FetchTasks()
        {
            var user = RetrieveUser();

            var favoriteList = _database.TaskFavorites.Where(x => x.UserId == user.Id).Select(x => x.TaskId).ToList();

            var tasks = _database.Task
                .Select(x => new TaskResponseDto
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
                    Locked = x.Locked,
                    Project = new ProjectResponseDto
                    {
                        Id = x.ProjectNavigation.Id,
                        Name = x.ProjectNavigation.Name,
                        Customer = new CustomerDto
                        {
                            Id = x.ProjectNavigation.CustomerNavigation.Id,
                            Name = x.ProjectNavigation.CustomerNavigation.Name
                        }
                    },
                })
                .ToList();

            tasks.ForEach(x => x.Favorite = favoriteList.Contains(x.Id) ? true : false);

            return Ok(tasks);
        }

        [HttpPost("Tasks")]
        [Authorize]
        public ActionResult<IEnumerable<TaskResponseDto>> UpdateFavoriteTasks([FromBody] IEnumerable<UpdateTasksDto> tasksToBeUpdated)
        {
            var user = RetrieveUser();

            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeUpdated)
            {
                TaskFavorites favoriteEntry = checkExisting.RetrieveExistingFavorite(task, user);

                if (task.Favorite == true && favoriteEntry == null)
                {
                    CreateNewFavorite(task, user);
                }
                else if (task.Favorite == false && favoriteEntry != null)
                {
                    _database.TaskFavorites.Remove(favoriteEntry);
                    _database.SaveChanges();
                }
                response.Add(returnObjects.ReturnTask(user, task));
            }
            return Ok(response);
        }

        private User RetrieveUser()
        {
            var username = User.Claims.FirstOrDefault(x => x.Type == "name").Value;
            var email = User.Claims.FirstOrDefault(x => x.Type == "preferred_username").Value;
            var alvUser = _database.User.FirstOrDefault(x => x.Email.Equals(email));

            return alvUser;
        }

        private TaskFavorites CreateNewFavorite(UpdateTasksDto taskDto, User user)
        {
            TaskFavorites favorite = new TaskFavorites
            {
                TaskId = taskDto.Id,
                UserId = user.Id
            };
            _database.TaskFavorites.Add(favorite);
            _database.SaveChanges();
            return favorite;
        }
    }
}
