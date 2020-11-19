using System;
using System.Collections.Generic;
using System.Text;

namespace AlvTime.Business.Customers
{
    public interface ICustomerStorage
    {
        IEnumerable<CustomerDto> GetCustomers(CustomerQuerySearch criterias);
        void CreateCustomer(CustomerDto customer);
        void UpdateCustomer(CustomerDto customer);
    }

    public class CustomerQuerySearch
    {
        public string Name { get; set; }
        public int? Id { get; set; }
    }
}
