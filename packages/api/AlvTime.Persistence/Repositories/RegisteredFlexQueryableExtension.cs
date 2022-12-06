using System.Linq;
using AlvTime.Business.TimeRegistration;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories;

public static class RegisteredFlexQueryableExtension
{
    public static IQueryable<RegisteredFlex> Filter(this IQueryable<RegisteredFlex> query, TimeEntryQuerySearch criterias)
    {
        if (criterias.UserId != null)
        {
            query = query.Where(hour => hour.UserId == criterias.UserId);
        }
        if (criterias.FromDateInclusive != null)
        {
            query = query.Where(hour => hour.Date.Date >= criterias.FromDateInclusive);
        }
        if (criterias.ToDateInclusive != null)
        {
            query = query.Where(hour => hour.Date.Date <= criterias.ToDateInclusive);
        }
        if (criterias.Id != null)
        {
            query = query.Where(hour => hour.Id == criterias.Id);
        }
        if (criterias.Value != null)
        {
            query = query.Where(hour => hour.Value == criterias.Value);
        }

        return query;
    }
}