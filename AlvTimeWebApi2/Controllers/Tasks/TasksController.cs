using AlvTimeApi.Dto;
using AlvTimeWebApi2.DataBaseModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeApi.Controllers.Tasks
{
    [Route("api/user")]
    [ApiController]
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
                    Favorite = x.Favorite,
                    Id = x.Id,
                    Locked = x.Locked,
                    Name = x.Name,
                    ProjectNavigation = x.ProjectNavigation,
                    HourRate = x.HourRate,
                })
                .ToList();
            return Ok(tasks);
        }

        /// <summary>
        /// Changes favorite state of task
        /// </summary>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        [HttpPost("Tasks")]
        public ActionResult<TaskResponseDto> UpdateTask([FromBody] UpdateTasksDto taskDto)
        {
            var user = RetrieveUser();

            return Ok(_database.TaskFavorites.FirstOrDefault(t => t.Id == taskDto.Id && t.UserId == user.Id));
        }

        private User RetrieveUser()
        {

            //var username = HttpContext.User.Identity.Name ?? "NameNotFound";
            //var user = _database.User.FirstOrDefault(x => x.Email.Trim() == username.Trim());
            var user = _database.User.FirstOrDefault();

            return user;
        }
    }
}
