using AlvTime.Business.Tasks;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class TasksController : Controller
    {
        private readonly RetrieveUsers _userRetriever;
        private readonly ITaskStorage _taskStorage;
        private readonly FavoriteUpdater _updater;

        public TasksController(RetrieveUsers userRetriever, ITaskStorage taskStorage, FavoriteUpdater updater)
        {
            _userRetriever = userRetriever;
            _taskStorage = taskStorage;
            _updater = updater;
        }

        [HttpGet("Tasks")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<IEnumerable<TaskResponseDto>> FetchTasks()
        {
            var user = _userRetriever.RetrieveUser();

            if (user == null)
            {
                return BadRequest("User not found");
            }

            var tasks = _taskStorage.GetUsersTasks(new TaskQuerySearch(), user.Id);

            return Ok(tasks);
        }

        [HttpPost("Tasks")]
        [Authorize(Policy = "AllowPersonalAccessToken")]
        public ActionResult<IEnumerable<TaskResponseDto>> UpdateFavoriteTasks([FromBody] IEnumerable<UpdateTasksDto> tasksToBeUpdated)
        {
            var user = _userRetriever.RetrieveUser();
            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeUpdated)
            {
                response.Add(_updater.UpdateFavoriteTasks(task, user.Id));
            }

            return Ok(response);
        }
    }
}
