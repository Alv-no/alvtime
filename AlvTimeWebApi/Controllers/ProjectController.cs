using AlvTime.Business.Projects;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class ProjectController : Controller
    {
        private readonly IProjectStorage _storage;
        private readonly ProjectCreator _creator;

        public ProjectController(IProjectStorage storage, ProjectCreator creator)
        {
            _storage = storage;
            _creator = creator;
        }

        [HttpGet("Projects")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<ProjectResponseDto>> FetchProjects()
        {
            return Ok(_storage.GetProjects(new ProjectQuerySearch()));
        }

        [HttpPost("CreateProject")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<ProjectResponseDto>> CreateNewProject([FromBody] IEnumerable<CreateProjectDto> projectsToBeCreated)
        {
            List<ProjectResponseDto> response = new List<ProjectResponseDto>();

            foreach (var project in projectsToBeCreated)
            {
                response.Add(_creator.CreateProject(project));
            }

            return Ok(response);
        }
    }
}
