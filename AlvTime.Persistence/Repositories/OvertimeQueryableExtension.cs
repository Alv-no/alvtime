using AlvTime.Business.FlexiHours;
using AlvTime.Persistence.DataBaseModels;
using System.Linq;

namespace AlvTime.Persistence.Repositories
{
    public static class OvertimeQueryableExtension
    {
        public static IQueryable<PaidOvertime> Filter(this IQueryable<PaidOvertime> query, OvertimePayoutQuerySearch criterias)
        {
            if (criterias.UserId != null)
            {
                query = query.Where(hour => hour.User == criterias.UserId);
            }
            if (criterias.FromDateInclusive != null)
            {
                query = query.Where(hour => hour.Date >= criterias.FromDateInclusive);
            }
            if (criterias.ToDateInclusive != null)
            {
                query = query.Where(hour => hour.Date <= criterias.ToDateInclusive);
            }

            return query;
        }
    }
}
