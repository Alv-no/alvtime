using AlvTime.Business.Tasks;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[AuthorizeAdmin]
public class TaskController : ControllerBase
{
    private readonly TaskService _taskService;

    public TaskController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost("Tasks")]
    public async Task<ActionResult<TaskResponse>> CreateNewTask([FromBody] TaskUpsertRequest taskToBeCreated, [FromQuery] int projectId)
    {
        var createdTask = await _taskService.CreateTask(taskToBeCreated.MapToTaskDto(), projectId);
        return Ok(createdTask.MapToTaskResponseSimple());
    }

    [HttpPut("Tasks/{taskId:int}")]
    public async Task<ActionResult<TaskResponse>> UpdateTask([FromBody] TaskUpsertRequest taskToBeUpdated, int taskId)
    {
        var updatedTask = await _taskService.UpdateTask(taskToBeUpdated.MapToTaskDto(taskId));
        return Ok(updatedTask.MapToTaskResponseSimple());
    }
}