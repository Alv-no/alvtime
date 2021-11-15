using AlvTime.Business.CompensationRate;
using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class TaskAdminController : Controller
    {
        private readonly TaskService _taskService;

        public TaskAdminController(ICompensationRateStorage compensationRateStorage, TaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("Tasks")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<TaskResponse>> FetchTasks()
        {
            var tasks = _taskService.GetAllTasks(new TaskQuerySearch { });
            return Ok(tasks.Select(task => new TaskResponse(task.Id, task.Name, task.Description, task.Favorite,
                task.Locked, task.CompensationRate, task.Project)));
        }

        [HttpPost("Tasks")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<TaskResponse>> CreateNewTask(
            [FromBody] IEnumerable<TaskCreateRequest> tasksToBeCreated)
        {
            var createdTasks = _taskService.CreateTasks(tasksToBeCreated.Select(task => new CreateTaskDto(
                task.Name,
                task.Description,
                task.Project,
                task.Locked,
                task.CompensationRate)));

            return Ok(createdTasks.Select(task => new TaskResponse(
                task.Id,
                task.Name,
                task.Description,
                task.Favorite,
                task.Locked,
                task.CompensationRate,
                task.Project)));
        }

        [HttpPut("Tasks")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<TaskResponse>> UpdateTask(
            [FromBody] IEnumerable<TaskUpdateRequest> tasksToBeUpdated)
        {
            var updatedTasks = _taskService.UpdateTasks(tasksToBeUpdated.Select(task => new UpdateTaskDto(
                task.Id,
                task.Locked,
                task.Name,
                task.CompensationRate)));

            return Ok(updatedTasks.Select(task => new TaskResponse(
                task.Id,
                task.Name,
                task.Description,
                task.Favorite,
                task.Locked,
                task.CompensationRate,
                task.Project)));
        }
    }
}