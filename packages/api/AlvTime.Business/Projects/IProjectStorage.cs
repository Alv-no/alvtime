using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.Projects
{
    public interface IProjectStorage
    {
        Task<IEnumerable<ProjectResponseDto>> GetProjects(ProjectQuerySearch criterias);
        Task UpdateProject(CreateProjectDto request);
        Task CreateProject(CreateProjectDto project);

    }

    public class ProjectQuerySearch
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int? Customer { get; set; }
    }
}
