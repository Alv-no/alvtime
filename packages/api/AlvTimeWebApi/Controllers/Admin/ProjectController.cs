using AlvTime.Business.Projects;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[AuthorizeAdmin]
public class ProjectController : Controller
{
    private readonly IProjectStorage _projectStorage;
    private readonly ProjectService _projectService;

    public ProjectController(IProjectStorage projectStorage, ProjectService projectService)
    {
        _projectStorage = projectStorage;
        _projectService = projectService;
    }

    [HttpGet("Projects")]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> FetchProjects()
    {
        return Ok(await _projectStorage.GetProjects(new ProjectQuerySearch()));
    }

    [HttpPost("Projects")]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> CreateNewProject([FromBody] IEnumerable<CreateProjectDto> projectsToBeCreated)
    {
        List<ProjectResponseDto> response = new List<ProjectResponseDto>();

        foreach (var project in projectsToBeCreated)
        {
            response.Add(await _projectService.CreateProject(project));
        }

        return Ok(response);
    }

    [HttpPut("Projects")]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> UpdateProject([FromBody] IEnumerable<CreateProjectDto> projectsToBeCreated)
    {
        List<ProjectResponseDto> response = new List<ProjectResponseDto>();

        foreach (var project in projectsToBeCreated)
        {
            response.Add(await _projectService.UpdateProject(project));
        }

        return Ok(response);
    }
}