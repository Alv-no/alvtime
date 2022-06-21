using System;
using AlvTime.Business.Interfaces;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories
{
    public class DbContextScope : IDbContextScope
    {
        private readonly AlvTime_dbContext _dbContext;

        public DbContextScope(AlvTime_dbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public void AsAtomic(Action atomicAction)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                atomicAction.Invoke();
                transaction.Commit();
            }
        }
    }
}