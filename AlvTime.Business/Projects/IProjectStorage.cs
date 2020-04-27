using System;
using System.Collections.Generic;
using System.Text;

namespace AlvTime.Business.Projects
{
    public interface IProjectStorage
    {
        IEnumerable<ProjectResponseDto> GetProjects(ProjectQuerySearch criterias);
        void CreateProject(CreateProjectDto project);

    }

    public class ProjectQuerySearch
    {
        public string Name { get; set; }
        public int? Customer { get; set; }
    }
}
