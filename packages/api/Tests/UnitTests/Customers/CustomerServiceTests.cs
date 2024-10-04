using AlvTime.Business.Customers;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;
using Xunit;
using CustomerService = AlvTime.Persistence.Repositories.CustomerService;
using Task = System.Threading.Tasks.Task;

namespace Tests.UnitTests.Customers;

public class CustomerServiceTests
{
    private readonly AlvTime_dbContext _context;
    
    public CustomerServiceTests()
    {
        _context = new AlvTimeDbContextBuilder()
            .WithCustomers()
            .CreateDbContext();
    }
    
    [Fact]
    public async Task CreateCustomer_NameSpecified_CustomerWithNameIsCreated()
    {
        var customerService = CreateCustomerService(_context);

        await customerService.CreateCustomer(
            new CustomerDto
            {
                Name = "Test"
            });

        var newCustomers = await customerService.GetCustomers(new CustomerQuerySearch
        {
            Name = "Test"
        });

        Assert.Single(newCustomers);
    }

    [Fact]
    public async Task UpdateCustomer_ContactPersonProvided_UpdatesContactPerson()
    {
        var customerService = CreateCustomerService(_context);

        await customerService.UpdateCustomer(
            new CustomerDto
            {
                Id = 1,
                InvoiceAddress = "Testveien 1"
            });

        var customer = (await customerService.GetCustomers(new CustomerQuerySearch
        {
            Id = 1
        })).Single();

        Assert.Equal("Testveien 1", customer.InvoiceAddress);
    }

    public CustomerService CreateCustomerService(AlvTime_dbContext context)
    {
        return new CustomerService(context);
    }
}