using AlvTime.Business.Projects;
using AlvTime.Persistence.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.UnitTests.Projects;

public class ProjectStorageTests
{
    [Fact]
    public async Task CreateProject_NameSpecified_CustomerWithNameIsCreated()
    {
        var context = new AlvTimeDbContextBuilder().CreateDbContext();

        var storage = new ProjectStorage(context);
        var projectService = new ProjectService(storage);

        var previousProjectAmount = context.Project.ToList().Count();

        await projectService.CreateProject("Test", 1);

        var newProjectAmount = context.Project.ToList().Count;

        Assert.Equal(previousProjectAmount + 1, newProjectAmount);
    }
}