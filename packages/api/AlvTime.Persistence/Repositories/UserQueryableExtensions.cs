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
                query = query.Where(user => user.Email.ToLower().Equals(criterias.Email.ToLower()));
            }
            if (criterias.Id != null)
            {
                query = query.Where(user => user.Id == criterias.Id);
            }
            if (criterias.Name != null)
            {
                query = query.Where(user => user.Name.ToLower().Equals(criterias.Name.ToLower()));
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

            if (criterias.Oid != null)
            {
                query = query.Where(user => user.Oid == criterias.Oid);
            }

            return query;
        }
    }
}
