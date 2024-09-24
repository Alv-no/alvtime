using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTime.Business.Customers;

public class CustomerService
{
    private readonly ICustomerStorage _customerStorage;

    public CustomerService(ICustomerStorage customerStorage)
    {
        _customerStorage = customerStorage;
    }
    
    public async Task<Result<CustomerAdminDto>> GetCustomerDetailedById(int customerId)
    {
        return (await _customerStorage.GetCustomerDetailedById(customerId));
    }

    public async Task<IEnumerable<CustomerAdminDto>> GetCustomersDetailed()
    {
        var customers = await _customerStorage.GetCustomersDetailed();
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
        
        await _customerStorage.CreateCustomer(customer);
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
        
        await _customerStorage.UpdateCustomer(customer);
        return (await GetCustomer(customer.Name, customer.Id)).Single();
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
        return (await _customerStorage.GetCustomers(new CustomerQuerySearch
        {
            Name = customerName,
            Id = customerId
        })).ToList();
    }
}