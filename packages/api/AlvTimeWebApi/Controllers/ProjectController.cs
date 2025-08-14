using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business.Projects;
using AlvTimeWebApi.ErrorHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class ProjectController(ProjectService projectService) : ControllerBase
{
    [HttpGet("user/projects")]
    public async Task<ActionResult<IEnumerable<ProjectResponseDtoV2>>> FetchProjects()
    {
        var result = await projectService.GetProjectsWithTasks(new ProjectQuerySearch());
        return result.Match<ActionResult<IEnumerable<ProjectResponseDtoV2>>>(
            projects => Ok(projects),
            errors => BadRequest(errors.ToValidationProblemDetails("Hent prosjekter feilet med følgende feil")));
    }
}