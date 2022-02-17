using AlvTime.Business.Projects;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class ProjectController : Controller
    {
        private readonly ProjectService _projectService;

        public ProjectController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet("Projects")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<ProjectResponseDto>> FetchProjects()
        {
            return Ok(_projectService.GetProjects(new ProjectQuerySearch()));
        }

        [HttpPost("Projects")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<ProjectResponseDto>> CreateNewProject([FromBody] IEnumerable<CreateProjectDto> projectsToBeCreated)
        {
            List<ProjectResponseDto> response = new List<ProjectResponseDto>();

            foreach (var project in projectsToBeCreated)
            {
                response.Add(_projectService.CreateProject(project));
            }

            return Ok(response);
        }

        [HttpPut("Projects")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<ProjectResponseDto>> UpdateProject([FromBody] IEnumerable<CreateProjectDto> projectsToBeCreated)
        {
            List<ProjectResponseDto> response = new List<ProjectResponseDto>();

            foreach (var project in projectsToBeCreated)
            {
                response.Add(_projectService.UpdateProject(project));
            }

            return Ok(response);
        }
    }
}
