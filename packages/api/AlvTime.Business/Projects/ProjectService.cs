using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTime.Business.Projects;

public class ProjectService
{
    private readonly IProjectStorage _projectStorage;

    public ProjectService(IProjectStorage projectStorage)
    {
        _projectStorage = projectStorage;
    }

    public async Task<Result<ProjectDto>> CreateProject(string projectName, int customerId)
    {
        var projectAlreadyExists = (await GetProject(projectName, customerId)).Any();
        if (projectAlreadyExists)
        {
            return new List<Error> { new(ErrorCodes.EntityAlreadyExists, "Et prosjekt med det navnet finnes allerede på den kunden.") };
        }
        
        await _projectStorage.CreateProject(projectName, customerId);
        return (await GetProject(projectName, customerId)).SingleOrDefault();
    }

    public async Task<ProjectDto> UpdateProject(ProjectDto projectToUpdate)
    {
        await _projectStorage.UpdateProject(projectToUpdate);
        return (await GetProjectById(projectToUpdate.Id)).SingleOrDefault();
    }

    private async Task<IEnumerable<ProjectDto>> GetProject(string projectName, int customerId)
    {
        return (await _projectStorage.GetProjects(new ProjectQuerySearch
        {
            Customer = customerId,
            Name = projectName
        })).ToList();
    }

    private async Task<IEnumerable<ProjectDto>> GetProjectById(int id)
    {
        return (await _projectStorage.GetProjects(new ProjectQuerySearch
        {
            Id = id
        })).ToList();
    }
}