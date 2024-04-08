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

    public async Task<IEnumerable<CustomerAdminDto>> GetCustomersDetailed()
    {
        var customers = await _customerStorage.GetCustomersDetailed();
        return customers;
    }
    
    public async Task<Result<CustomerDto>> CreateCustomer(CustomerDto customer)
    {
        var customerAlreadyExists = (await GetCustomer(customer.Name, customer.Id)).Any();
        if (customerAlreadyExists)
        {
            return new List<Error> { new(ErrorCodes.EntityAlreadyExists, "En kunde med det navnet finnes allerede") };
        }
        
        await _customerStorage.CreateCustomer(customer);
        return (await GetCustomer(customer.Name, customer.Id)).Single();
    }
    
    public async Task<Result<CustomerDto>> UpdateCustomer(CustomerDto customer)
    {
        var customerAlreadyExists = (await GetCustomer(customer.Name, null)).Any();
        if (customerAlreadyExists)
        {
            return new List<Error> { new(ErrorCodes.EntityAlreadyExists, "En kunde med det navnet finnes allerede") };
        }
        
        await _customerStorage.UpdateCustomer(customer);
        return (await GetCustomer(customer.Name, customer.Id)).Single();
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