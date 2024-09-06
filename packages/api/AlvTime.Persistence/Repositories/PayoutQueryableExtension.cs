using System.Linq;
using AlvTime.Business.Payouts;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories
{
    public static class PayoutQueryableExtension
    {
        public static IQueryable<PaidOvertime> Filter(this IQueryable<PaidOvertime> query, PayoutQueryFilter criterias)
        {
            if (criterias.UserId != null)
            {
                query = query.Where(entry => entry.User == criterias.UserId);
            }
            if (criterias.FromDateInclusive != null)
            {
                query = query.Where(entry => entry.Date.Date >= criterias.FromDateInclusive);
            }
            if (criterias.ToDateInclusive != null)
            {
                query = query.Where(entry => entry.Date.Date <= criterias.ToDateInclusive);
            }
            if (criterias.IsLocked != null)
            {
                query = query.Where(entry => entry.IsLocked == criterias.IsLocked);
            }

            return query;
        }
    }
}