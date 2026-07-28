using AlvTime.Business.Customers;
using System;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.UnitTests.Customers;

public class CustomerServiceTests
{
    private readonly AlvTime_dbContext _context = new AlvTimeDbContextBuilder()
        .WithCustomers()
        .CreateDbContext();

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
                InvoiceAddress = "Testveien 123"
            });

        var customer = (await customerService.GetCustomers(new CustomerQuerySearch
        {
            Id = 1
        })).Single();

        Assert.Equal("Testveien 123", customer.InvoiceAddress);
    }

    [Fact]
    public async Task GetActiveCustomers_CustomerHasSeveralRecentEntries_CustomerIsReturnedOnce()
    {
        var customerStorage = CreateCustomerService(_context);
        SeedCustomerWithTimeEntries(customerId: 1, projectId: 1, taskId: 1,
            DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-3));

        var activeCustomers = await customerStorage.GetActiveCustomers();

        Assert.Single(activeCustomers, customer => customer.Id == 1);
    }

    [Fact]
    public async Task GetActiveCustomers_CustomerHasEntriesOnSeveralProjects_CustomerIsReturnedOnce()
    {
        var customerStorage = CreateCustomerService(_context);
        SeedCustomerWithTimeEntries(customerId: 1, projectId: 1, taskId: 1, DateTime.Now.AddDays(-1));
        SeedCustomerWithTimeEntries(customerId: 1, projectId: 2, taskId: 2, DateTime.Now.AddDays(-1));

        var activeCustomers = await customerStorage.GetActiveCustomers();

        Assert.Single(activeCustomers, customer => customer.Id == 1);
    }

    [Fact]
    public async Task GetActiveCustomers_CustomerOnlyHasOldEntries_CustomerIsNotReturned()
    {
        var customerStorage = CreateCustomerService(_context);
        SeedCustomerWithTimeEntries(customerId: 1, projectId: 1, taskId: 1, DateTime.Now.AddDays(-1));
        SeedCustomerWithTimeEntries(customerId: 2, projectId: 2, taskId: 2, DateTime.Now.AddMonths(-6));

        var activeCustomers = await customerStorage.GetActiveCustomers();

        Assert.DoesNotContain(activeCustomers, customer => customer.Id == 2);
    }

    [Fact]
    public async Task GetActiveCustomers_CustomerIsLocked_LockedToIsIncluded()
    {
        var customerStorage = CreateCustomerService(_context);
        SeedCustomerWithTimeEntries(customerId: 1, projectId: 1, taskId: 1, DateTime.Now.AddDays(-1));
        await customerStorage.LockCustomer(new DateTime(2026, 04, 30), 1);

        var activeCustomers = await customerStorage.GetActiveCustomers();

        Assert.Equal(new DateTime(2026, 04, 30), activeCustomers.Single(customer => customer.Id == 1).LockedTo);
    }

    private void SeedCustomerWithTimeEntries(int customerId, int projectId, int taskId, params DateTime[] entryDates)
    {
        if (_context.Customer.All(customer => customer.Id != customerId))
        {
            _context.Customer.Add(new Customer { Id = customerId, Name = $"Customer{customerId}" });
        }

        _context.Project.Add(new Project { Id = projectId, Name = $"Project{projectId}", Customer = customerId });
        _context.Task.Add(new AlvTime.Persistence.DatabaseModels.Task { Id = taskId, Name = $"Task{taskId}", Description = "", Project = projectId });

        foreach (var entryDate in entryDates)
        {
            _context.Hours.Add(new Hours
            {
                User = 1,
                Date = entryDate,
                DayNumber = (short)entryDate.DayOfYear,
                Year = (short)entryDate.Year,
                TaskId = taskId,
                Value = 7.5M
            });
        }

        _context.SaveChanges();
    }

    public CustomerStorage CreateCustomerService(AlvTime_dbContext context)
    {
        return new CustomerStorage(context);
    }
}