using System.Linq;
using AlvTime.Business.TimeRegistration;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories
{
    public static class OvertimeQueryableExtension
    {
        public static IQueryable<EarnedOvertime> Filter(this IQueryable<EarnedOvertime> query, OvertimeQueryFilter criterias)
        {
            if (criterias.UserId != null)
            {
                query = query.Where(entry => entry.UserId == criterias.UserId);
            }
            if (criterias.FromDateInclusive != null)
            {
                query = query.Where(entry => entry.Date.Date >= criterias.FromDateInclusive);
            }
            if (criterias.ToDateInclusive != null)
            {
                query = query.Where(entry => entry.Date.Date <= criterias.ToDateInclusive);
            }
            if (criterias.CompensationRate != null)
            {
                query = query.Where(entry => entry.CompensationRate == criterias.CompensationRate);
            }

            return query;
        }
    }
}