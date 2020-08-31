using System.Collections.Generic;

namespace AlvTime.Business.Projects
{
    public interface IProjectStorage
    {
        IEnumerable<ProjectResponseDto> GetProjects(ProjectQuerySearch criterias);
        void UpdateProject(CreateProjectDto request);
        void CreateProject(CreateProjectDto project);

    }

    public class ProjectQuerySearch
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int? Customer { get; set; }
    }
}
