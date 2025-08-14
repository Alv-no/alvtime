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

[Route("api")]
[ApiController]
[Authorize]
public class TasksController(TaskService taskService) : ControllerBase
{
    [HttpGet("user/Tasks")]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> FetchTasks()
    {
        var result = await taskService.GetTasksForUser(new TaskQuerySearch());
        return result.Match<ActionResult<IEnumerable<TaskResponse>>>(
            tasks => Ok(tasks.Select(task => new TaskResponse(task.Id, task.Name, task.Description, task.Favorite,
                task.Locked, task.CompensationRate, task.Project))),
            errors => BadRequest(errors.ToValidationProblemDetails("Hent tasks feilet med følgende feil")));
    }

    [HttpGet("user/LastUsedTasks")]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> FetchLastUsedTasks()
    {
        return Ok(await taskService.GetLatestTasksForUser());
    }

    [HttpPost("user/Tasks")]
    public async Task<ActionResult> UpdateFavoriteTasks(
        [FromBody] IEnumerable<TaskFavoriteRequest> tasksToBeUpdated)
    {
        await taskService.UpdateFavoriteTasks(tasksToBeUpdated.Select(t =>
            new UpdateTaskDto(Id: t.Id, Favorite: t.Favorite, EnableComments: t.EnableComments)));

        return NoContent();
    }
}