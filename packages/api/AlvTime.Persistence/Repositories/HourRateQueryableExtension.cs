using AlvTime.Business.HourRates;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories;

public static class HourRateQueryableExtension
{
    public static IQueryable<HourRate> Filter(this IQueryable<HourRate> query, HourRateQuerySearch criterias)
    {
        if (criterias.Id != null)
        {
            query = query.Where(hourRate => hourRate.Id == criterias.Id);
        }
        if (criterias.FromDate != null)
        {
            query = query.Where(hourRate => hourRate.FromDate == criterias.FromDate);
        }
        if (criterias.Rate != null)
        {
            query = query.Where(hourRate => hourRate.Rate == criterias.Rate);
        }

        return query;
    }
}