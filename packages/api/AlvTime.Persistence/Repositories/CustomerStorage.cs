using AlvTime.Business.Customers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence.Repositories
{
    public class CustomerStorage : ICustomerStorage
    {
        private readonly AlvTime_dbContext _context;

        public CustomerStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public async Task CreateCustomer(CustomerDto customer)
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
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerDto>> GetCustomers(CustomerQuerySearch criterias)
        {
            return await _context.Customer.AsQueryable()
                .Filter(criterias)
                .Select(x => new CustomerDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ContactPerson = x.ContactPerson,
                    ContactEmail = x.ContactEmail,
                    ContactPhone = x.ContactPhone,
                    InvoiceAddress = x.InvoiceAddress
                }).ToListAsync();
        }

        public async Task UpdateCustomer(CustomerDto customer)
        {
            var existingCustomer = await _context.Customer
                .FirstOrDefaultAsync(x => x.Id == customer.Id);

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

            await _context.SaveChangesAsync();
        }
    }
}
