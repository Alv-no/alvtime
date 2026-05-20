using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business.Users;

namespace AlvTime.Business.Projects;

public interface IProjectStorage
{
    Task<IEnumerable<ProjectDto>> GetProjects(ProjectQuerySearch criteria);
    Task<IEnumerable<ProjectResponseDtoV2>> GetProjectsWithTasksForUser(ProjectQuerySearch criteria, User user);
    Task UpdateProject(ProjectDto request);
    Task CreateProject(string projectName, int customerId);
    Task UpdateProjectFavorites(IEnumerable<ProjectFavoriteDto> projectFavorites, int userId);

}

public class ProjectQuerySearch
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public int? Customer { get; set; }
}