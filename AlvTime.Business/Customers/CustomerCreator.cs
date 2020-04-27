using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlvTime.Business.Customers
{
    public class CustomerCreator
    {
        private readonly ICustomerStorage _storage;

        public CustomerCreator(ICustomerStorage storage)
        {
            _storage = storage;
        }

        public CustomerDto CreateCustomer(CustomerDto customer)
        {
            if (!GetCustomer(customer).Any())
            {
                _storage.CreateCustomer(customer);
            }

            return GetCustomer(customer).Single();
        }

        public CustomerDto UpdateCustomer(CustomerDto customer)
        {
            _storage.UpdateCustomer(customer);

            return GetCustomer(customer).Single();
        }

        public IEnumerable<CustomerDto> GetCustomer(CustomerDto customer)
        {
            return _storage.GetCustomers(new CustomerQuerySearch
            {
                Name = customer.Name,
                Id = customer.Id
            }).ToList();
        }
    }
}
