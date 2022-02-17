using System.Collections.Generic;
using System.Linq;

namespace AlvTime.Business.Projects
{
    public class ProjectService
    {
        private readonly IProjectStorage _storage;

        public ProjectService(IProjectStorage storage)
        {
            _storage = storage;
        }

        public ProjectResponseDto CreateProject(CreateProjectDto project)
        {
            ProjectQuerySearch criterias = new ProjectQuerySearch
            {
                Customer = project.Customer,
                Name = project.Name
            };
            if (!GetProjects(criterias).Any())
            {
                _storage.CreateProject(project);
            }

            return GetProjects(criterias).SingleOrDefault();
        }

        public ProjectResponseDto UpdateProject(CreateProjectDto project)
        {
            _storage.UpdateProject(project);

            return GetProjects(new ProjectQuerySearch
            {
                Id = project.Id
            }).SingleOrDefault();
        }
        public IEnumerable<ProjectResponseDto> GetProjects(ProjectQuerySearch criterias)
        {
            return _storage.GetProjects(criterias);
        }
    }
}
