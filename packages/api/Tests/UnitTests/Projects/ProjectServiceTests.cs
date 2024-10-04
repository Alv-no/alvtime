using AlvTime.Business.Projects;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Xunit;
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

        var created = (await projectService.CreateProject("Test", 1)).Value;

        Assert.Equal(1, created.Id);
    }

    private ProjectService CreateProjectService(AlvTime_dbContext context)
    {
        var storage = new ProjectStorage(context);
        return new ProjectService(storage);
    }
}