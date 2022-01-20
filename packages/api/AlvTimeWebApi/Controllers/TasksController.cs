using AlvTime.Business.Tasks;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using AlvTimeWebApi.Requests;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class TasksController : Controller
    {
        private readonly TaskService _taskService;

        public TasksController(TaskService taskService)
        {
            _taskService = taskService;
        }
 
        [HttpGet("Tasks")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<IEnumerable<TaskResponse>> FetchTasks()
        {
            return Ok(_taskService.GetTasks()
                .Select(task => new TaskResponse(task.Id, task.Name, task.Description, task.Favorite, task.Locked, task.CompensationRate, task.Project)));
        }

        [HttpPost("Tasks")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<IEnumerable<TaskResponse>> UpdateFavoriteTasks([FromBody] IEnumerable<TaskFavoriteRequest> tasksToBeUpdated)
        {
            var updatedTasks = _taskService.UpdateFavoriteTasks(tasksToBeUpdated.Select(t => (t.Id, t.Favorite)));
            return Ok(updatedTasks.Select(task => new TaskResponse(task.Id, task.Name, task.Description, task.Favorite, task.Locked, task.CompensationRate, task.Project)));
        }
    }
}
