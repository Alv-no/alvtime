using System.Linq;
using AlvTime.Business.Overtime;
using AlvTime.Persistence.DataBaseModels;

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
            if (criterias.Date != null)
            {
                query = query.Where(entry => entry.Date.Date >= criterias.Date);
            }
            if (criterias.CompensationRate != null)
            {
                query = query.Where(entry => entry.CompensationRate == criterias.CompensationRate);
            }

            return query;
        }
    }
}