using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.DatabaseModels;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class ProjectController : Controller
    {
        private readonly AlvTime_dbContext _database;

        private CreatedObjectReturner returnObjects;
        private ExistingObjectFinder checkExisting;

        public ProjectController(AlvTime_dbContext database)
        {
            _database = database;
            returnObjects = new CreatedObjectReturner(_database);
            checkExisting = new ExistingObjectFinder(_database);
        }

        [HttpGet("Projects")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<ProjectResponseDto>> FetchProjects()
        {
            var projects = _database.Project
                .Select(x => new ProjectResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Customer = new CustomerDto
                    {
                        Id = x.CustomerNavigation.Id,
                        Name = x.CustomerNavigation.Name,
                        ContactEmail = x.CustomerNavigation.ContactEmail,
                        ContactPerson = x.CustomerNavigation.ContactPerson,
                        ContactPhone = x.CustomerNavigation.ContactPhone,
                        InvoiceAddress = x.CustomerNavigation.InvoiceAddress
                    }
                }).ToList();

            return Ok(projects);
        }

        [HttpPost("CreateProject")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<ProjectResponseDto>> CreateNewProject([FromBody] IEnumerable<CreateProjectDto> projectsToBeCreated)
        {
            List<ProjectResponseDto> response = new List<ProjectResponseDto>();

            foreach (var project in projectsToBeCreated)
            {
                if (checkExisting.ProjectDoesNotExist(project))
                {
                    var newProject = new Project
                    {
                        Customer = project.Customer,
                        Name = project.Name,
                    };
                    _database.Project.Add(newProject);
                    _database.SaveChanges();

                    response.Add(returnObjects.ReturnCreatedProject(project));
                }
            }
            return Ok(response);
        }
    }
}
