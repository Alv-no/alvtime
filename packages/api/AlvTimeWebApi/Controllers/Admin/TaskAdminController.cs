using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[AuthorizeAdmin]
public class TaskAdminController : Controller
{
    private readonly TaskService _taskService;

    public TaskAdminController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("Tasks")]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> FetchTasks()
    {
        var tasks = await _taskService.GetAllTasks(new TaskQuerySearch());
        return Ok(tasks.Select(task => new TaskResponse(task.Id, task.Name, task.Description, task.Favorite,
            task.Locked, task.CompensationRate, task.Project)));
    }

    [HttpPost("Tasks")]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> CreateNewTask(
        [FromBody] IEnumerable<TaskCreateRequest> tasksToBeCreated)
    {
        var createdTasks = await _taskService.CreateTasks(tasksToBeCreated.Select(task => new CreateTaskDto(
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
    public async Task<ActionResult<IEnumerable<TaskResponse>>> UpdateTask(
        [FromBody] IEnumerable<TaskUpdateRequest> tasksToBeUpdated)
    {
        var updatedTasks = await _taskService.UpdateTasks(tasksToBeUpdated.Select(task => new UpdateTaskDto(
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