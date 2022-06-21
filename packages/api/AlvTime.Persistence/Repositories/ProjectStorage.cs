using AlvTime.Business.Customers;
using AlvTime.Business.Projects;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories
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
                Customer = (int)newProject.Customer,
                Name = newProject.Name
            };

            _context.Project.Add(project);
            _context.SaveChanges();
        }

        public void UpdateProject(CreateProjectDto request)
        {
            var existingProject = _context.Project
                   .Where(x => x.Id == request.Id)
                   .FirstOrDefault();

            if (request.Customer != null)
            {
                existingProject.Customer = (int)request.Customer;
            }
            if (request.Name != null)
            {
                existingProject.Name = request.Name;
            }

            _context.SaveChanges();
        }
    }
}
