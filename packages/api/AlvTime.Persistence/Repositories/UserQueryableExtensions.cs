﻿using AlvTime.Business.Users;
using AlvTime.Persistence.DataBaseModels;
using System.Linq;

namespace AlvTime.Persistence.Repositories
{
    public static class UserQueryableExtensions
    {
        public static IQueryable<User> Filter(this IQueryable<User> query, UserQuerySearch criterias)
        {
            if (criterias.Email != null)
            {
                query = query.Where(user => user.Email == criterias.Email);
            }
            if (criterias.Id != null)
            {
                query = query.Where(user => user.Id == criterias.Id);
            }
            if (criterias.Name != null)
            {
                query = query.Where(user => user.Name == criterias.Name);
            }
            if (criterias.StartDate != null)
            {
                query = query.Where(user => user.StartDate >= criterias.StartDate);
            }
            if (criterias.EndDate != null)
            {
                query = query.Where(user => user.EndDate <= criterias.EndDate);
            }

            return query;
        }
    }
}
