using AlvTimeApi.Dto;
using AlvTimeWebApi2.DataBaseModels;
using AlvTimeWebApi2.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeApi.Controllers.Tasks
{
    [Route("api/user")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = "AzureAd")]
    public class TasksController : Controller
    {
        private readonly AlvTimeDBContext _database;

        public TasksController(AlvTimeDBContext database)
        {
            _database = database;
        }

        /// <summary>
        /// Retrieves tasks
        /// </summary>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        [HttpGet("Tasks")]
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
                    Favorite = favoriteList.Contains(x.Id) ? true : false,
                    Project = new ProjectDto
                    {
                        Id = x.ProjectNavigation.Id,
                        Name = x.ProjectNavigation.Name,
                        Customer = new CustomerDto
                        {
                            Id = x.ProjectNavigation.CustomerNavigation.Id,
                            Name = x.ProjectNavigation.CustomerNavigation.Name
                        }
                    },
                    HourRate = x.HourRate,
                })
                .ToList();
            return Ok(tasks);
        }

        /// <summary>
        /// Changes favorite state of task for user
        /// </summary>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        [HttpPost("Tasks")]
        public ActionResult<List<TaskResponseDto>> UpdateFavoriteTasks([FromBody] List<UpdateTasksDto> tasksToBeUpdated)
        {
            var user = RetrieveUser();

            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeUpdated)
            {
                TaskFavorites favoriteEntry = RetrieveExistingFavorite(task, user);

                if (task.Favorite == true)
                {
                    if (favoriteEntry == null)
                    {
                        CreateNewFavorite(task, user);
                    }
                }
                else if (task.Favorite == false)
                {
                    if (favoriteEntry != null)
                    {
                        _database.TaskFavorites.Remove(favoriteEntry);
                        _database.SaveChanges();
                    }
                }
                response.Add(ReturnTask(user, task));
            }
            return Ok(response);
        }

        private TaskResponseDto ReturnTask(User user, UpdateTasksDto task)
        {
            var userHasFavorite = _database.TaskFavorites.FirstOrDefault(x => x.TaskId == task.Id && x.UserId == user.Id);

            var taskResponseDto = _database.Task
                .Where(x => x.Id == task.Id)
                .Select(x => new TaskResponseDto
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
                    Locked = x.Locked,
                    Favorite = userHasFavorite == null ? false : true,
                    Project = new ProjectDto
                    {
                        Id = x.ProjectNavigation.Id,
                        Name = x.ProjectNavigation.Name,
                        Customer = new CustomerDto
                        {
                            Id = x.ProjectNavigation.CustomerNavigation.Id,
                            Name = x.ProjectNavigation.CustomerNavigation.Name
                        }
                    },
                    HourRate = x.HourRate,
                })
                .Single();

            return taskResponseDto;
        }

        private User RetrieveUser()
        {

            //var username = HttpContext.User.Identity.Name ?? "NameNotFound";
            //var user = _database.User.FirstOrDefault(x => x.Email.Trim() == username.Trim());
            var user = _database.User.FirstOrDefault();

            return user;
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

        private TaskFavorites RetrieveExistingFavorite(UpdateTasksDto taskDto, User user)
        {
            return _database.TaskFavorites.FirstOrDefault(x =>
                x.TaskId == taskDto.Id &&
                x.UserId == user.Id);
        }
    }
}
