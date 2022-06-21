using AlvTime.Business.Customers;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.Persistence.Repositories
{
    public class CustomerStorage : ICustomerStorage
    {
        private readonly AlvTime_dbContext _context;

        public CustomerStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public void CreateCustomer(CustomerDto customer)
        {
            var newCustomer = new Customer
            {
                Name = customer.Name,
                InvoiceAddress = customer.InvoiceAddress != null ? customer.InvoiceAddress : "",
                ContactPhone = customer.ContactPhone != null ? customer.ContactPhone : "",
                ContactEmail = customer.ContactEmail != null ? customer.ContactEmail : "",
                ContactPerson = customer.ContactPerson != null ? customer.ContactPerson : ""
            };

            _context.Customer.Add(newCustomer);
            _context.SaveChanges();
        }

        public IEnumerable<CustomerDto> GetCustomers(CustomerQuerySearch criterias)
        {
            return _context.Customer.AsQueryable()
                .Filter(criterias)
                .Select(x => new CustomerDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ContactPerson = x.ContactPerson,
                    ContactEmail = x.ContactEmail,
                    ContactPhone = x.ContactPhone,
                    InvoiceAddress = x.InvoiceAddress
                }).ToList();
        }

        public void UpdateCustomer(CustomerDto customer)
        {
            var existingCustomer = _context.Customer
                .Where(x => x.Id == customer.Id)
                .FirstOrDefault();

            if (customer.Name != null)
            {
                existingCustomer.Name = customer.Name;
            }
            if (customer.ContactEmail != null)
            {
                existingCustomer.ContactEmail = customer.ContactEmail;
            }
            if (customer.ContactPerson != null)
            {
                existingCustomer.ContactPerson = customer.ContactPerson;
            }
            if (customer.ContactPhone != null)
            {
                existingCustomer.ContactPhone = customer.ContactPhone;
            }
            if (customer.InvoiceAddress != null)
            {
                existingCustomer.InvoiceAddress = customer.InvoiceAddress;
            }

            _context.SaveChanges();
        }
    }
}
