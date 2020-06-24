using AlvTime.Business.Customers;
using AlvTime.Persistence.Repositories;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.Customers
{
    public class CustomerStorageTests
    {
        [Fact]
        public void CreateCustomer_NameSpecified_CustomerWithNameIsCreated()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var storage = new CustomerStorage(context);
            var creator = new CustomerCreator(storage);

            var previousCustomersAmount = context.Customer.ToList().Count();

            creator.CreateCustomer(new CustomerDto
            {
                Name = "Test"
            });

            var newCustomersAmount = context.Customer.ToList().Count();

            Assert.Equal(previousCustomersAmount+1, newCustomersAmount);
        }

        [Fact]
        public void UpdateCustomer_ContactPersonProvided_UpdatesContactPerson()
        {
            var context = new AlvTimeDbContextBuilder().WithData().CreateDbContext();

            var storage = new CustomerStorage(context);
            var creator = new CustomerCreator(storage);

            creator.UpdateCustomer(new CustomerDto
            {
                Id = 1,
                InvoiceAddress = "Testveien 1"
            });

            var customer = storage.GetCustomers(new CustomerQuerySearch
            {
                Id = 1
            }).Single();

            Assert.Equal("Testveien 1", customer.InvoiceAddress);
        }
    }
}
