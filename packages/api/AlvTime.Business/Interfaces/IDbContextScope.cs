using System;

namespace AlvTime.Business.Interfaces
{
    public interface IDbContextScope
    {
        void AsAtomic(Action atomicAction);
    }
}