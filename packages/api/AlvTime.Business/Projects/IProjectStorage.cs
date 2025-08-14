using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.Projects;

public interface IProjectStorage
{
    Task<IEnumerable<ProjectDto>> GetProjects(ProjectQuerySearch criteria);
    Task<IEnumerable<ProjectResponseDtoV2>> GetProjectsWithTasks(ProjectQuerySearch criteria, int userId);
    Task UpdateProject(ProjectDto request);
    Task CreateProject(string projectName, int customerId);

}

public class ProjectQuerySearch
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public int? Customer { get; set; }
}