using AlvTime.Business.Customers;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Requests;
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
    public async Task<ActionResult<IEnumerable<CustomerDetailedResponse>>> FetchCustomersDetailed()
    {
        var customers = await _customerService.GetCustomersDetailed();
        return Ok(customers.Select(c => c.MapToCustomerResponse()));
    }

    [HttpPost("Customers")]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> CreateNewCustomers([FromBody] IEnumerable<CustomerCreateRequest> customersToBeCreated)
    {
        var createdCustomers = await _customerService.CreateCustomers(customersToBeCreated.Select(c => c.MapToCustomerDto()));
        return Ok(createdCustomers.Select(c => c.MapToCustomerResponse()));
    }

    [HttpPut("Customers")]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> UpdateExistingCustomers([FromBody] IEnumerable<CustomerUpdateRequest> customersToBeUpdated)
    {
        var updatedCustomers = await _customerService.UpdateCustomers(customersToBeUpdated.Select(c => c.MapToCustomerDto()));
        return Ok(updatedCustomers.Select(c => c.MapToCustomerResponse()));
    }
}