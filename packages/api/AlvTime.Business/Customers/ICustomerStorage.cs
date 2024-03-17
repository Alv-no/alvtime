using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.Customers
{
    public interface ICustomerStorage
    {
        Task<IEnumerable<CustomerDto>> GetCustomers(CustomerQuerySearch criterias);
        Task<IEnumerable<CustomerAdminDto>> GetCustomersDetailed();
        Task CreateCustomer(CustomerDto customer);
        Task UpdateCustomer(CustomerDto customer);
    }

    public class CustomerQuerySearch
    {
        public string Name { get; set; }
        public int? Id { get; set; }
    }
}
