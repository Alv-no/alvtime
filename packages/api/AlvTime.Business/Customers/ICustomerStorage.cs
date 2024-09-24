using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.Customers
{
    public interface ICustomerStorage
    {
        Task<CustomerAdminDto> GetCustomerDetailedById(int customerId);
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
