using AlvTime.Business.Customers;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Responses.Admin;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[AuthorizeAdmin]
public class CustomerController : Controller
{
    private readonly CustomerService _customerService;

    public CustomerController(CustomerService customerService)
    {
        _customerService = customerService;
    }
    
    [HttpGet("Customers")]
    public async Task<ActionResult<IEnumerable<CustomerAdminResponse>>> FetchCustomersDetailed()
    {
        var customers = await _customerService.GetCustomersDetailed();
        return Ok(customers.Select(c => c.MapToCustomerResponse()));
    }

    [HttpPost("Customers")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> CreateNewCustomers([FromBody] IEnumerable<CustomerDto> customersToBeCreated)
    {
        List<CustomerDto> response = new List<CustomerDto>();

        foreach (var customer in customersToBeCreated)
        {
            response.Add(await _customerService.CreateCustomer(customer));
        }

        return Ok(response);
    }

    [HttpPut("Customers")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> UpdateExistingCustomers([FromBody] IEnumerable<CustomerDto> customersToBeUpdated)
    {
        List<CustomerDto> response = new List<CustomerDto>();

        foreach (var customer in customersToBeUpdated)
        {
            response.Add(await _customerService.UpdateCustomer(customer));
        }
        return Ok(response);
    }
}