using AlvTime.Business.HourRates;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin.HourRates.HourRateStorage
{
    public static class HourRateQueryableExtension
    {
        public static IQueryable<HourRate> Filter(this IQueryable<HourRate> query, HourRateQuerySearch criterias)
        {
            if (criterias.FromDate != null)
            {
                query = query.Where(hourRate => hourRate.FromDate == criterias.FromDate);
            }
            if (criterias.Rate != null)
            {
                query = query.Where(hourRate => hourRate.Rate == criterias.Rate);
            }
            if (criterias.TaskId != null)
            {
                query = query.Where(hourRate => hourRate.TaskId == criterias.TaskId);
            }

            return query;
        }
    }
}
