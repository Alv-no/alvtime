﻿using System;
using AlvTime.Business.Users;
using System.Linq;
using User = AlvTime.Persistence.DatabaseModels.User;

namespace AlvTime.Persistence.Repositories
{
    public static class UserQueryableExtensions
    {
        public static IQueryable<User> Filter(this IQueryable<User> query, UserQuerySearch criterias)
        {
            if (criterias.Email != null)
            {
                query = query.Where(user => user.Email.Equals(criterias.Email, StringComparison.InvariantCultureIgnoreCase));
            }
            if (criterias.Id != null)
            {
                query = query.Where(user => user.Id == criterias.Id);
            }
            if (criterias.Name != null)
            {
                query = query.Where(user => user.Name.Equals(criterias.Name, StringComparison.InvariantCultureIgnoreCase));
            }
            if (criterias.StartDate != null)
            {
                query = query.Where(user => user.StartDate >= criterias.StartDate);
            }
            if (criterias.EndDate != null)
            {
                query = query.Where(user => user.EndDate <= criterias.EndDate);
            }
            if (criterias.EmployeeId != null)
            {
                query = query.Where(user => user.EmployeeId == criterias.EmployeeId);
            }

            return query;
        }
    }
}
