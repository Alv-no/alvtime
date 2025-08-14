using AlvTime.Business.Customers;
using AlvTime.Business.Projects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories;

public class ProjectStorage : IProjectStorage
{
    private readonly AlvTime_dbContext _context;

    public ProjectStorage(AlvTime_dbContext context)
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
    
    public async Task<IEnumerable<ProjectResponseDtoV2>> GetProjectsWithTasks(ProjectQuerySearch criteria, int userId)
    {
        var projects = await _context.Project.AsQueryable().AsNoTracking()
            .Filter(criteria)
            .Include(p => p.CustomerNavigation)
            .Include(p => p.Task)
            .ThenInclude(t => t.CompensationRate)
            .Select(p => new ProjectResponseDtoV2
            {
                Name = p.Name,
                CustomerName = p.CustomerNavigation.Name,
                Tasks = p.Task.Select(t => new TaskResponseDtoV2
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Favorite = _context.TaskFavorites.Any(fav => fav.UserId == userId && fav.TaskId == t.Id),
                    Locked = t.Locked,
                    Imposed = t.Imposed,
                    CompensationRate = t.CompensationRate
                        .OrderByDescending(cr => cr.FromDate)
                        .Select(cr => cr.Value)
                        .FirstOrDefault()
                })
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