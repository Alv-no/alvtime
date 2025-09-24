using AlvTime.Business.Projects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories;

public class ProjectStorage(AlvTime_dbContext context) : IProjectStorage
{
    public async Task<IEnumerable<ProjectDto>> GetProjects(ProjectQuerySearch criteria)
    {
        var projects = await context.Project.AsQueryable()
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
        var projects = await context.Project.AsQueryable().AsNoTracking()
            .Filter(criteria)
            .Include(p => p.CustomerNavigation)
            .Include(p => p.Task)
            .ThenInclude(t => t.CompensationRate)
            .Include(p => p.ProjectFavorites)
            .Select(p => new ProjectResponseDtoV2
            {
                Id = p.Id,
                Name = p.Name,
                CustomerName = p.CustomerNavigation.Name,
                Index = p.ProjectFavorites
                    .Where(pf => pf.UserId == userId && pf.ProjectId == p.Id)
                    .Select(pf => (int?)pf.Index)
                    .FirstOrDefault() ?? 9999,
                Tasks = p.Task.Select(t => new TaskResponseDtoV2
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Favorite = context.TaskFavorites.Any(fav => fav.UserId == userId && fav.TaskId == t.Id),
                    Locked = t.Locked,
                    Imposed = t.Imposed,
                    CompensationRate = t.CompensationRate
                        .OrderByDescending(cr => cr.FromDate)
                        .Select(cr => cr.Value)
                        .FirstOrDefault(),
                    EnableComments = context.TaskFavorites.Any(fav => fav.UserId == userId && fav.TaskId == t.Id && fav.EnableComments),
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

        context.Project.Add(project);
        await context.SaveChangesAsync();
    }

    public async Task UpdateProjectFavorites(IEnumerable<ProjectFavoriteDto> projectFavorites, int userId)
    {
        await context.ProjectFavorite.Where(pf => pf.UserId == userId).ExecuteDeleteAsync();
        foreach (var projectFavorite in projectFavorites)
        {
            await context.ProjectFavorite.AddAsync(new ProjectFavorites
            {
                UserId = userId,
                ProjectId = projectFavorite.Id,
                Index = projectFavorite.Index,
            });
        }
        await context.SaveChangesAsync();
    }

    public async Task UpdateProject(ProjectDto request)
    {
        var existingProject = await context.Project
            .FirstOrDefaultAsync(x => x.Id == request.Id);

        if (request.Name != null)
        {
            existingProject.Name = request.Name;
        }

        await context.SaveChangesAsync();
    }
}