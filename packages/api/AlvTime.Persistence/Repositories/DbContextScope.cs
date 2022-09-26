using System;
using AlvTime.Business.Interfaces;
using AlvTime.Persistence.DatabaseModels;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories
{
    public class DbContextScope : IDbContextScope
    {
        private readonly AlvTime_dbContext _dbContext;

        public DbContextScope(AlvTime_dbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task AsAtomic(Func<Task> atomicAction)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            await atomicAction.Invoke();
            await transaction.CommitAsync();
        }
    }
}