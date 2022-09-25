using AlvTime.Business.Customers;
using AlvTime.Persistence.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.UnitTests.Customers
{
    public class CustomerStorageTests
    {
        [Fact]
        public async Task CreateCustomer_NameSpecified_CustomerWithNameIsCreated()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new CustomerStorage(context);
            var customerService = new CustomerService(storage);

            var previousCustomersAmount = context.Customer.ToList().Count();

            await customerService.CreateCustomer(new CustomerDto
            {
                Name = "Test"
            });

            var newCustomersAmount = context.Customer.ToList().Count();

            Assert.Equal(previousCustomersAmount+1, newCustomersAmount);
        }

        [Fact]
        public async Task UpdateCustomer_ContactPersonProvided_UpdatesContactPerson()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithCustomers()
                .CreateDbContext();

            var storage = new CustomerStorage(context);
            var customerService = new CustomerService(storage);

            await customerService.UpdateCustomer(new CustomerDto
            {
                Id = 1,
                InvoiceAddress = "Testveien 1"
            });

            var customer = (await storage.GetCustomers(new CustomerQuerySearch
            {
                Id = 1
            })).Single();

            Assert.Equal("Testveien 1", customer.InvoiceAddress);
        }
    }
}
