using AlvTime.Business.Customers;
using AlvTime.Business.Projects;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin.Projects.ProjectStorage
{
    public class ProjectStorage : IProjectStorage
    {
        private readonly AlvTime_dbContext _context;

        public ProjectStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public IEnumerable<ProjectResponseDto> GetProjects(ProjectQuerySearch criterias)
        {
            var projects = _context.Project.AsQueryable()
                .Filter(criterias)
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

            return projects;
        }

        public void CreateProject(CreateProjectDto newProject)
        {
            var project = new Project
            {
                Customer = newProject.Customer,
                Name = newProject.Name
            };

            _context.Project.Add(project);
            _context.SaveChanges();
        }
    }
}
