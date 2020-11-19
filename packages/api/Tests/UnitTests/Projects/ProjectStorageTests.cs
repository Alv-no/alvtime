using AlvTime.Business.Projects;
using AlvTime.Persistence.Repositories;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.Projects
{
    public class ProjectStorageTests
    {
        [Fact]
        public void CreateProject_NameSpecified_CustomerWithNameIsCreated()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new ProjectStorage(context);
            var creator = new ProjectCreator(storage);

            var previousProjectAmount = context.Project.ToList().Count();

            creator.CreateProject(new CreateProjectDto
            {
                Name = "Test",
                Customer = 1
            });

            var newProjectAmount = context.Project.ToList().Count();

            Assert.Equal(previousProjectAmount + 1, newProjectAmount);
        }
    }
}
