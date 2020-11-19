using AlvTime.Business.CompensationRate;
using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class TaskAdminController : Controller
    {
        private readonly TaskCreator _creator;
        private readonly ITaskStorage _taskStorage;
        private readonly ICompensationRateStorage _compensationRateStorage;

        public TaskAdminController(TaskCreator creator, ITaskStorage taskStorage, ICompensationRateStorage compensationRateStorage)
        {
            _creator = creator;
            _taskStorage = taskStorage;
            _compensationRateStorage = compensationRateStorage;
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
