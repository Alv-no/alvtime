using AlvTime.Business.Customers;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin.Customers.CustomerStorage
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
