using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlvTime.Business.Customers
{
    public class CustomerService
    {
        private readonly ICustomerStorage _storage;

        public CustomerService(ICustomerStorage storage)
        {
            _storage = storage;
        }

        public CustomerDto CreateCustomer(CustomerDto customer)
        {
            CustomerQuerySearch criterias = new CustomerQuerySearch
            {
                Name = customer.Name,
                Id = customer.Id
            };
            if (!GetCustomers(criterias).Any())
            {
                _storage.CreateCustomer(customer);
            }

            return GetCustomers(criterias).Single();
        }

        public CustomerDto UpdateCustomer(CustomerDto customer)
        {
            _storage.UpdateCustomer(customer);

            return GetCustomers(new CustomerQuerySearch{
                Name = customer.Name,
                Id = customer.Id
            }).Single();
        }

        public IEnumerable<CustomerDto> GetCustomers(CustomerQuerySearch criterias)
        {
            return _storage.GetCustomers(criterias);
        }
    }
}
