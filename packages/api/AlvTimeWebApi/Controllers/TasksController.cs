using AlvTime.Business.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.ErrorHandling;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers;

[Route("api/user")]
[ApiController]
[Authorize]
public class TasksController : Controller
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("Tasks")]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> FetchTasks()
    {
        var result = await _taskService.GetTasksForUser(new TaskQuerySearch());
        return result.Match<ActionResult<IEnumerable<TaskResponse>>>(
            tasks => Ok(tasks.Select(task => new TaskResponse(task.Id, task.Name, task.Description, task.Favorite, task.Locked, task.CompensationRate, task.Project))),
            errors => BadRequest(errors.ToValidationProblemDetails("Hent tasks feilet med følgende feil")));
    }

    [HttpGet("LastUsedTasks")]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> FetchLastUsedTasks()
    {
        return Ok(await _taskService.GetLatestTasksForUser());
    }

    [HttpPost("Tasks")]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> UpdateFavoriteTasks(
        [FromBody] IEnumerable<TaskFavoriteRequest> tasksToBeUpdated)
    {
        var result = await _taskService.UpdateFavoriteTasks(tasksToBeUpdated.Select(t => (t.Id, t.Favorite)));
        return result.Match<ActionResult<IEnumerable<TaskResponse>>>(
            tasks => Ok(tasks.Select(task => new TaskResponse(task.Id, task.Name, task.Description, task.Favorite, task.Locked, task.CompensationRate, task.Project))),
            errors => BadRequest(errors.ToValidationProblemDetails("Hent tasks feilet med følgende feil")));
    }
}