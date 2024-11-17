using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Projects;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.ErrorHandling;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Responses.Admin;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ProjectController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost("Projects")]
    public async Task<ActionResult<ProjectResponse>> CreateNewProject([FromBody] ProjectUpsertRequest projectToBeCreated, [FromQuery] int customerId)
    {
        var result = await _projectService.CreateProject(projectToBeCreated.Name, customerId);
        return result.Match<ActionResult<ProjectResponse>>(
            project => Ok(project.MapToProjectResponse()),
            errors => BadRequest(errors.ToValidationProblemDetails("Opprettelse av prosjekt feilet")));
    }

    [HttpPut("Projects/{projectId:int}")]
    public async Task<ActionResult<ProjectResponse>> UpdateProject([FromBody] ProjectUpsertRequest projectToBeCreated, int projectId)
    {
        var updatedProject = await _projectService.UpdateProject(projectToBeCreated.MapToProjectDto(projectId));
        return Ok(updatedProject.MapToProjectResponse());
    }
}