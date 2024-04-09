using System;
using AlvTime.Business.Customers;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories
{
    public static class CustomerQueryableExtension
    {
        public static IQueryable<Customer> Filter(this IQueryable<Customer> query, CustomerQuerySearch criterias)
        {
            if (criterias.Name != null)
            {
                query = query.Where(customer => customer.Name.ToLower().Equals(criterias.Name.ToLower()));
            }
            if (criterias.Id != null)
            {
                query = query.Where(customer => customer.Id == criterias.Id);
            }

            return query;
        }
    }
}
