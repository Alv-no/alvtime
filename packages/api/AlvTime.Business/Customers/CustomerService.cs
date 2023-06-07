using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTime.Business.Customers
{
    public class CustomerService
    {
        private readonly ICustomerStorage _customerStorage;

        public CustomerService(ICustomerStorage customerStorage)
        {
            _customerStorage = customerStorage;
        }

        public async Task<CustomerDto> CreateCustomer(CustomerDto customer)
        {
            var customerAlreadyExists = (await GetCustomer(customer)).Any();
            if (!customerAlreadyExists)
            {
                await _customerStorage.CreateCustomer(customer);
            }

            return (await GetCustomer(customer)).Single();
        }

        public async Task<CustomerDto> UpdateCustomer(CustomerDto customer)
        {
            await _customerStorage.UpdateCustomer(customer);

            return (await GetCustomer(customer)).Single();
        }

        private async Task<IEnumerable<CustomerDto>> GetCustomer(CustomerDto customer)
        {
            return (await _customerStorage.GetCustomers(new CustomerQuerySearch
            {
                Name = customer.Name,
                Id = customer.Id
            })).ToList();
        }
    }
}
