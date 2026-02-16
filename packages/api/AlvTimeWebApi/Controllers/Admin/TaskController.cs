using AlvTime.Business.Tasks;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Responses.Admin;
using AlvTime.Business;
using AlvTimeWebApi.ErrorHandling;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Policy = "AdminPolicy")]
public class TaskController : ControllerBase
{
    private readonly TaskService _taskService;

    public TaskController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost("Tasks")]
    public async Task<ActionResult<TaskResponseSimple>> CreateNewTask([FromBody] TaskUpsertRequest taskToBeCreated, [FromQuery] int projectId)
    {
        var result = await _taskService.CreateTask(taskToBeCreated.MapToTaskDto(), projectId);
        return result.Match<ActionResult<TaskResponseSimple>>(
            createdTask => Ok(createdTask.MapToTaskResponseSimple()),
            errors => BadRequest(errors.ToValidationProblemDetails("Oppdater task feilet med følgende feil")));
    }

    [HttpPut("Tasks/{taskId:int}")]
    public async Task<ActionResult<TaskResponseSimple>> UpdateTask([FromBody] TaskUpsertRequest taskToBeUpdated, int taskId)
    {
        var result = await _taskService.UpdateTask(taskToBeUpdated.MapToTaskDto(taskId));
        return result.Match<ActionResult<TaskResponseSimple>>(
            UpdateTask => Ok(UpdateTask.MapToTaskResponseSimple()),
            errors => BadRequest(errors.ToValidationProblemDetails("Oppdater task feilet med følgende feil")));
    }
}