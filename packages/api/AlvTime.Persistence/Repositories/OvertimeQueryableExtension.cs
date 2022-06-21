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
            if (criterias.StartDate != null)
            {
                query = query.Where(entry => entry.Date.Date >= criterias.StartDate);
            }
            if (criterias.EndDate != null)
            {
                query = query.Where(entry => entry.Date.Date <= criterias.EndDate);
            }
            if (criterias.CompensationRate != null)
            {
                query = query.Where(entry => entry.CompensationRate == criterias.CompensationRate);
            }

            return query;
        }
    }
}