using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class TaskAdminController : Controller
    {
        private readonly TaskCreator _creator;
        private RetrieveUsers _userRetriever;
        private ITaskStorage _taskStorage;

        public TaskAdminController(RetrieveUsers userRetriever, TaskCreator creator, ITaskStorage taskStorage)
        {
            _userRetriever = userRetriever;
            _creator = creator;
            _taskStorage = taskStorage;
        }

        [HttpGet("Tasks")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<TaskResponseDto>> FetchTasks()
        {
            var tasks = _taskStorage.GetTasks(new TaskQuerySearch {});
            return Ok(tasks);
        }


        [HttpPost("Tasks")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<TaskResponseDto>> CreateNewTask([FromBody] IEnumerable<CreateTaskDto> tasksToBeCreated)
        {
            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeCreated)
            {
                response.Add(_creator.CreateTask(task));
            }
            return Ok(response);
        }

        [HttpPut("Tasks")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<TaskResponseDto>> UpdateTask([FromBody] IEnumerable<UpdateTasksDto> tasksToBeUpdated)
        {
            List<TaskResponseDto> response = new List<TaskResponseDto>();

            foreach (var task in tasksToBeUpdated)
            {
                response.Add(_creator.UpdateTask(task));
            }
            return Ok(response);
        }
    }
}
