using System;
using System.Threading.Tasks;
using AlvTime.Business.Interfaces;
using AlvTime.Persistence.DatabaseModels;
using Wolverine.EntityFrameworkCore;
using Wolverine.Runtime;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories
{
    public class DbContextScope : IDbContextScope
    {
        private readonly AlvTime_dbContext _dbContext;
        private readonly IDbContextOutbox<AlvTime_dbContext> _outbox;

        public DbContextScope(AlvTime_dbContext dbContext, IDbContextOutbox<AlvTime_dbContext> outbox)
        {
            _dbContext = dbContext;
            _outbox = outbox;

            // One request can save several time entries, each in its own AsAtomic with its own flush.
            // Wolverine only sends on the first flush by default, so allow it to send on every flush.
            if (outbox is MessageContext context)
            {
                context.MultiFlushMode = MultiFlushMode.AllowMultiples;
            }
        }

        public async Task AsAtomic(Func<Task> atomicAction)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            await atomicAction.Invoke();
            await _dbContext.SaveChangesAsync(); // persists outbox envelopes in the transaction
            await transaction.CommitAsync();
            await _outbox.FlushOutgoingMessagesAsync();
        }

        public ValueTask PublishAsync<T>(T message) => _outbox.PublishAsync(message);
    }
}
