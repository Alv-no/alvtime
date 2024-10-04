using AlvTime.Business.Projects;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;
using Xunit;
using ProjectService = AlvTime.Persistence.Repositories.ProjectService;
using Task = System.Threading.Tasks.Task;

namespace Tests.UnitTests.Projects;

public class ProjectServiceTests
{
    private readonly AlvTime_dbContext _context;
    
    public ProjectServiceTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .CreateDbContext();
    }
    
    [Fact]
    public async Task CreateProject_NameSpecified_CustomerWithNameIsCreated()
    {
        var projectService = CreateProjectService(_context);

        var projectsBefore = await projectService.GetProjects(new ProjectQuerySearch());

        await projectService.CreateProject("Test", 1);

        var projectsAfter = await projectService.GetProjects(new ProjectQuerySearch());

        Assert.Equal(projectsBefore.Count() + 1, projectsAfter.Count());
    }

    private ProjectService CreateProjectService(AlvTime_dbContext context)
    {
        return new ProjectService(context);
    }
}