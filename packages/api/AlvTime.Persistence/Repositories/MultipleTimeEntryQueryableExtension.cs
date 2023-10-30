using System.Linq;
using AlvTime.Business.TimeRegistration;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories
{
    public static class MultipleTimeEntryQueryableExtension
    {
        public static IQueryable<Hours> Filter(this IQueryable<Hours> query, MultipleTimeEntriesQuerySearch criterias)
        {
            if (criterias.EmployeeIds.Any())
            {
                query = query.Where(hour => criterias.EmployeeIds.Any(user => user == hour.UserNavigation.EmployeeId));
            }
            if (criterias.FromDateInclusive != null)
            {
                query = query.Where(hour => hour.Date.Date >= criterias.FromDateInclusive);
            }
            if (criterias.ToDateInclusive != null)
            {
                query = query.Where(hour => hour.Date.Date <= criterias.ToDateInclusive);
            }
            if (criterias.TaskIds.Any())
            {
                query = query.Where(hour => criterias.TaskIds.Any(task => task == hour.TaskId));
            }

            return query;
        }
    }
}
