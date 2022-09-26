using System;
using System.Threading.Tasks;

namespace AlvTime.Business.Interfaces
{
    public interface IDbContextScope
    {
        Task AsAtomic(Func<Task> atomicAction);
    }
}