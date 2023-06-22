using AlvTime.Business.AssociatedTask;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[AuthorizeAdmin]
public class AssociatedTaskController : Controller
{
    private readonly IAssociatedTaskStorage _associatedTaskStorage;
    private readonly AssociatedTaskService _associatedTaskService;

    public AssociatedTaskController(IAssociatedTaskStorage associatedTaskStorage, AssociatedTaskService associatedTaskService)
    {
        _associatedTaskStorage = associatedTaskStorage;
        _associatedTaskService = associatedTaskService;
    }

    [HttpGet("AssociatedTasks")]
    public async Task<ActionResult<IEnumerable<AssociatedTaskResponseDto>>> FetchAssociatedTasks()
    {
        var tasks = await _associatedTaskStorage.GetAssociatedTasks();

        return Ok(tasks);
    }

    [HttpPost("AssociatedTasks")]
    public async Task<ActionResult<IEnumerable<AssociatedTaskResponseDto>>> CreateAssociatedTask([FromBody] IEnumerable<AssociatedTaskRequestDto> associatedTasks)
    {
        List<AssociatedTaskResponseDto> response = new();

        foreach (var associatedTask in associatedTasks)
        {
            response.Add(await _associatedTaskService.CreateAssociatedTask(associatedTask));
        }

        return Ok(response);
    }

    [HttpPut("AssociatedTasks")]
    public async Task<ActionResult<IEnumerable<AssociatedTaskResponseDto>>> UpdateAssociatedTask([FromBody] IEnumerable<AssociatedTaskUpdateDto> associatedTasks)
    {
        List<AssociatedTaskResponseDto> response = new();

        foreach (var associatedTask in associatedTasks)
        {
            response.Add(await _associatedTaskService.UpdateAssociatedTask(associatedTask));
        }

        return Ok(response);
    }
}