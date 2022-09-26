using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTime.Business.Projects
{
    public class ProjectService
    {
        private readonly IProjectStorage _projectStorage;

        public ProjectService(IProjectStorage projectStorage)
        {
            _projectStorage = projectStorage;
        }

        public async Task<ProjectResponseDto> CreateProject(CreateProjectDto project)
        {
            var projectAlreadyExists = (await GetProject(project)).Any();
            if (!projectAlreadyExists)
            {
                await _projectStorage.CreateProject(project);
            }

            return (await GetProject(project)).SingleOrDefault();
        }

        public async Task<ProjectResponseDto> UpdateProject(CreateProjectDto project)
        {
            await _projectStorage.UpdateProject(project);

            return (await GetProjectById(project)).SingleOrDefault();
        }

        private async Task<IEnumerable<ProjectResponseDto>> GetProject(CreateProjectDto project)
        {
            return (await _projectStorage.GetProjects(new ProjectQuerySearch
            {
                Customer = project.Customer,
                Name = project.Name
            })).ToList();
        }

        private async Task<IEnumerable<ProjectResponseDto>> GetProjectById(CreateProjectDto project)
        {
            return (await _projectStorage.GetProjects(new ProjectQuerySearch
            {
                Id = project.Id
            })).ToList();
        }
    }
}
