using System;
using AlvTime.Business.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.TimeRegistration;
using AlvTimeWebApi.Authorization;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTime.Business;
using AlvTimeWebApi.ErrorHandling;

namespace AlvTimeWebApi.Controllers;

[Route("api/user")]
[ApiController]
[AuthorizePersonalAccessToken]
public class TasksController : Controller
{
    private readonly TaskService _taskService;
    private readonly LatestTaskService _latestTaskService;

    public TasksController(TaskService taskService, LatestTaskService latestTaskService)
    {
        _taskService = taskService;
        _latestTaskService = latestTaskService;
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
        return Ok(await _latestTaskService.GetLatestTasksForUser());
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