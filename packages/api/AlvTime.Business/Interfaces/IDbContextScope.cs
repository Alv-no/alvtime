using System;
using System.Threading.Tasks;

namespace AlvTime.Business.Interfaces
{
    public interface IDbContextScope
    {
        Task AsAtomic(Func<Task> atomicAction);

        // Stored in the same transaction as AsAtomic, sent after commit
        ValueTask PublishAsync<T>(T message);
    }
}