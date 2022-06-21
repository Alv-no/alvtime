using AlvTime.Business.CompensationRate;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories
{
    public static class CompensationRateQueryableExtensions
    {
        public static IQueryable<CompensationRate> Filter(this IQueryable<CompensationRate> query, CompensationRateQuerySearch criterias)
        {
            if (criterias.FromDate != null)
            {
                query = query.Where(cr => cr.FromDate == criterias.FromDate);
            }
            if (criterias.TaskId != null)
            {
                query = query.Where(cr => cr.TaskId == criterias.TaskId);
            }
            if (criterias.Value != null)
            {
                query = query.Where(cr => cr.Value == criterias.Value);
            }
            return query;
        }
    }
}
