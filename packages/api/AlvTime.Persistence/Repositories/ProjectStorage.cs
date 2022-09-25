using AlvTime.Business.Customers;
using AlvTime.Business.Projects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories
{
    public class ProjectStorage : IProjectStorage
    {
        private readonly AlvTime_dbContext _context;

        public ProjectStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectResponseDto>> GetProjects(ProjectQuerySearch criterias)
        {
            var projects = await _context.Project.AsQueryable()
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
                }).ToListAsync();

            return projects;
        }

        public async Task CreateProject(CreateProjectDto newProject)
        {
            var project = new Project
            {
                Customer = (int)newProject.Customer,
                Name = newProject.Name
            };

            _context.Project.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProject(CreateProjectDto request)
        {
            var existingProject = await _context.Project
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (request.Customer != null)
            {
                existingProject.Customer = (int)request.Customer;
            }
            if (request.Name != null)
            {
                existingProject.Name = request.Name;
            }

            await _context.SaveChangesAsync();
        }
    }
}
