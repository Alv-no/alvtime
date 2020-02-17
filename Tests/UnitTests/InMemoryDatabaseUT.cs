using AlvTimeApi.Controllers.Tasks;
using AlvTimeWebApi2.DataBaseModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace Tests
{
    public class InMemoryDatabaseUT
    {
        [Fact]
        public void InMemoryDatabaseRuns()
        {
            var options = new DbContextOptionsBuilder<AlvTimeDBContext>()
                            .UseInMemoryDatabase(databaseName: "Database_runs")
                            .Options;

            // Run the test against one instance of the context
            using (var context = new AlvTimeDBContext(options))
            {
                var service = new TasksController(context);

                var ha = new TaskFavorites
                {
                    TaskId = 2,
                    UserId = 3
                };

                context.Add(ha);

                context.SaveChanges();
            }

            using (var context = new AlvTimeDBContext(options))
            {
                Assert.Equal(1, context.TaskFavorites.Count());
            }
        }
    }
}
