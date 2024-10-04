using AlvTime.Business.Customers;
using AlvTime.Business.Projects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories;

public class ProjectService : IProjectStorage
{
    private readonly AlvTime_dbContext _context;

    public ProjectService(AlvTime_dbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectDto>> GetProjects(ProjectQuerySearch criteria)
    {
        var projects = await _context.Project.AsQueryable()
            .Filter(criteria)
            .Select(x => new ProjectDto
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();

        return projects;
    }

    public async Task CreateProject(string projectName, int customerId)
    {
        var project = new Project
        {
            Customer = customerId,
            Name = projectName
        };

        _context.Project.Add(project);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProject(ProjectDto request)
    {
        var existingProject = await _context.Project
            .FirstOrDefaultAsync(x => x.Id == request.Id);

        if (request.Name != null)
        {
            existingProject.Name = request.Name;
        }

        await _context.SaveChangesAsync();
    }
}