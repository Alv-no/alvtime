using System;
using System.Linq;
using AlvTime.Business.Users;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories;

public static class EmploymentRateQueryableExtension
{
    public static IQueryable<EmploymentRate> Filter(this IQueryable<EmploymentRate> query, EmploymentRateQueryFilter criterias)
    {
        if (criterias.Rate != null)
        {
            query = query.Where(er => er.Rate == criterias.Rate);
        }

        if (criterias.UserId != null)
        {
            query = query.Where(er => er.UserId == criterias.UserId);
        }

        return query;
    }
}