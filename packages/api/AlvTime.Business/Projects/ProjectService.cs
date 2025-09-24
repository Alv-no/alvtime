using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Users;

namespace AlvTime.Business.Projects;

public class ProjectService(IProjectStorage projectStorage, IUserContext userContext)
{
    public async Task<Result<IEnumerable<ProjectResponseDtoV2>>> GetProjectsWithTasks(ProjectQuerySearch criteria)
    {
        var currentUser = await userContext.GetCurrentUser();
        var projects = await projectStorage.GetProjectsWithTasks(criteria, currentUser.Id);
        var orderedProjects = projects.OrderBy(p => p.Index).ThenBy(p => p.Name);
        return new Result<IEnumerable<ProjectResponseDtoV2>>(orderedProjects);
    }
    
    public async Task<Result<ProjectDto>> CreateProject(string projectName, int customerId)
    {
        var projectAlreadyExists = (await GetProject(projectName, customerId)).Any();
        if (projectAlreadyExists)
        {
            return new List<Error> { new(ErrorCodes.EntityAlreadyExists, "Et prosjekt med det navnet finnes allerede på den kunden.") };
        }
        
        await projectStorage.CreateProject(projectName, customerId);
        return (await GetProject(projectName, customerId)).SingleOrDefault();
    }

    public async Task<ProjectDto> UpdateProject(ProjectDto projectToUpdate)
    {
        await projectStorage.UpdateProject(projectToUpdate);
        return (await GetProjectById(projectToUpdate.Id)).SingleOrDefault();
    }

    public async Task UpdateProjectFavorites(IEnumerable<ProjectFavoriteDto> projectFavorites)
    {
        var user = await userContext.GetCurrentUser();
        await projectStorage.UpdateProjectFavorites(projectFavorites, user.Id);
    }

    private async Task<IEnumerable<ProjectDto>> GetProject(string projectName, int customerId)
    {
        return (await projectStorage.GetProjects(new ProjectQuerySearch
        {
            Customer = customerId,
            Name = projectName
        })).ToList();
    }

    private async Task<IEnumerable<ProjectDto>> GetProjectById(int id)
    {
        return (await projectStorage.GetProjects(new ProjectQuerySearch
        {
            Id = id
        })).ToList();
    }
}