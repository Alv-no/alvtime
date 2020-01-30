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

            var tasks = _database.Task
                .Select(x => new TaskResponseDto
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
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
        /// Retrieves favorite tasks for user
        /// </summary>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        [HttpGet("FavoriteTasks")]
        public ActionResult<IEnumerable<FavoriteTasksDto>> FetchFavoriteTasks()
        {
            var user = RetrieveUser();

            var favorites = _database.TaskFavorites
                .Where(x => x.UserId == user.Id)
                .Select(x => new FavoriteTasksDto
                {
                    Id = x.Id,
                    TaskId = x.TaskId,
                    UserId = x.UserId
                })
                .ToList();
            return Ok(favorites);
        }

        /// <summary>
        /// Changes favorite state of task for user
        /// </summary>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        [HttpPost("FavoriteTasks")]
        public ActionResult<TaskResponseDto> UpdateFavoriteTasks([FromBody] UpdateTasksDto taskDto)
        {
            var user = RetrieveUser();

            TaskFavorites favoriteEntry = RetrieveExistingFavorite(taskDto, user);
            if (favoriteEntry == null)
            {
                favoriteEntry = CreateNewFavorite(taskDto, user);
            }

            return Ok(new TaskResponseDto
            {
                Favorite = RetrieveExistingFavorite(taskDto, user) == null ? false : true
            });
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
            return _database.TaskFavorites.FirstOrDefault(h =>
                h.TaskId == taskDto.Id &&
                h.UserId == user.Id);
        }
    }
}
