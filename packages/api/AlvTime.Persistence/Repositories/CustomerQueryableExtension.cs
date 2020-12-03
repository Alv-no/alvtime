using AlvTime.Business.Customers;
using AlvTime.Persistence.DataBaseModels;
using System.Linq;

namespace AlvTime.Persistence.Repositories
{
    public static class CustomerQueryableExtension
    {
        public static IQueryable<Customer> Filter(this IQueryable<Customer> query, CustomerQuerySearch criterias)
        {
            if (criterias.Name != null)
            {
                query = query.Where(customer => customer.Name == criterias.Name);
            }
            if (criterias.Id != null)
            {
                query = query.Where(customer => customer.Id == criterias.Id);
            }

            return query;
        }
    }
}
