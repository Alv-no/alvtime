using AlvTimeWebApi.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace Tests.UnitTests
{
    public class AlvTimeDbContextBuilder
    {
        public AlvTime_dbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AlvTime_dbContext>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;

            var context = new AlvTime_dbContext(options);
            return context;
        }
    }
}