using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DataBaseModels;
using System.Linq;

namespace AlvTime.Persistence.Repositories
{
    public static class TimeEntryQueryableExtension
    {
        public static IQueryable<Hours> Filter(this IQueryable<Hours> query, TimeEntryQuerySearch criterias)
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
            if (criterias.Id != null)
            {
                query = query.Where(hour => hour.Id == criterias.Id);
            }
            if (criterias.TaskId != null)
            {
                query = query.Where(hour => hour.TaskId == criterias.TaskId);
            }
            if (criterias.Value != null)
            {
                query = query.Where(hour => hour.Value == criterias.Value);
            }

            return query;
        }
    }
}
