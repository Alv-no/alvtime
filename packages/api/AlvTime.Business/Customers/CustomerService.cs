using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTime.Business.Customers;

public class CustomerService(ICustomerStorage customerStorage)
{
    public async Task<Result<CustomerAdminDto>> GetCustomerDetailedById(int customerId)
    {
        return await customerStorage.GetCustomerDetailedById(customerId);
    }

    public async Task<IEnumerable<CustomerAdminDto>> GetCustomersAdmin()
    {
        var customers = await customerStorage.GetCustomersAdmin();
        return customers;
    }

    public async Task<Result<CustomerDto>> CreateCustomer(CustomerDto customer)
    {
        var errors = new List<Error>();
        await ValidateCustomer(customer, errors);

        if (errors.Any())
        {
            return errors;
        }
        
        await customerStorage.CreateCustomer(customer);
        return (await GetCustomer(customer.Name, customer.Id)).Single();
    }

    public async Task<Result<CustomerDto>> UpdateCustomer(CustomerDto customer)
    {
        var errors = new List<Error>();
        await ValidateCustomer(customer, errors);
        
        if (errors.Any())
        {
            return errors;
        }
        
        await customerStorage.UpdateCustomer(customer);
        return (await GetCustomer(customer.Name, customer.Id)).Single();
    }

    public async Task LockCustomers(DateTime toDateInclusive, List<int> customersToExclude, int? customerId = null)
    {
        if (customerId == null)
        {
            var allCustomers = await customerStorage.GetCustomers(new CustomerQuerySearch());
            var customerIds  = allCustomers.Select(x => x.Id).Where(id => !customersToExclude.Contains(id!.Value)).ToList();

            foreach (var id in customerIds)
            {
                await customerStorage.LockCustomer(toDateInclusive, id!.Value);
            }
        }
        else
        {
            await customerStorage.LockCustomer(toDateInclusive, customerId.Value);
        }
    }

    public async Task UnlockCustomers(int? customerId = null)
    {
        if (customerId == null)
        {
            var allCustomers = await customerStorage.GetCustomers(new CustomerQuerySearch());
            var customerIds  = allCustomers.Select(x => x.Id).ToList();

            foreach (var id in customerIds)
            {
                await customerStorage.UnlockCustomer(id!.Value);
            }
        }
        else
        {
            await customerStorage.UnlockCustomer(customerId.Value);
        }
    }

    private async Task ValidateCustomer(CustomerDto customer, List<Error> errors)
    {
        var customerAlreadyExists = (await GetCustomer(customer.Name, null)).Any(c => c.Id != customer.Id);
        if (customerAlreadyExists)
        {
            errors.Add(new Error(ErrorCodes.EntityAlreadyExists, "En kunde med det navnet finnes allerede"));
        }
        
        if (customer.OrgNr != null && customer.OrgNr.Length != 9)
        {
            errors.Add(new Error(ErrorCodes.RequestInvalidProperty, "Organisasjonsnummer må være 9 tegn langt"));
        }
    }

    private async Task<IEnumerable<CustomerDto>> GetCustomer(string customerName, int? customerId)
    {
        return (await customerStorage.GetCustomers(new CustomerQuerySearch
        {
            Name = customerName,
            Id = customerId
        })).ToList();
    }
}