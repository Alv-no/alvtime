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
        public ActionResult<List<TaskResponseDto>> UpdateFavoriteTasks([FromBody] List<UpdateTasksDto> requests)
        {
            var user = RetrieveUser();

            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var request in requests)
            {
                TaskFavorites favoriteEntry = RetrieveExistingFavorite(request, user);

                if (request.Favorite == true)
                {
                    if (favoriteEntry == null)
                    {
                        favoriteEntry = CreateNewFavorite(request, user);
                        response.Add(ReturnTask(user, favoriteEntry));
                    }
                }
                else if (request.Favorite == false)
                {
                    if (favoriteEntry != null)
                    {
                        _database.Remove(favoriteEntry);
                        response.Add(ReturnTask(user, favoriteEntry));
                    }
                }
            }
            return Ok(response);
        }

        private TaskResponseDto ReturnTask(User user, TaskFavorites favoriteEntry)
        {
            var task = _database.Task.First(x => x.Id == favoriteEntry.TaskId);

            var taskResponseDto = new TaskResponseDto
            {
                Description = task.Description,
                Id = task.Id,
                Name = task.Name,
                Locked = task.Locked,
                Favorite = _database.TaskFavorites
                .Where(y => y.UserId == user.Id && y.TaskId == task.Id) == null ? false : true,
                Project = new ProjectDto
                {
                    Id = task.ProjectNavigation.Id,
                    Name = task.ProjectNavigation.Name,
                    Customer = new CustomerDto
                    {
                        Id = task.ProjectNavigation.CustomerNavigation.Id,
                        Name = task.ProjectNavigation.CustomerNavigation.Name
                    }
                },
                HourRate = task.HourRate,
            };

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
            _database.Add(favorite);
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
