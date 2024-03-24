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
    public async Task<ActionResult<CustomerResponse>> CreateNewCustomer([FromBody] CustomerUpsertRequest customerToBeCreated)
    {
        var createdCustomer = await _customerService.CreateCustomer(customerToBeCreated.MapToCustomerDto(null));
        return Ok(createdCustomer.MapToCustomerResponse());
    }

    [HttpPut("Customers/{customerId:int}")]
    public async Task<ActionResult<CustomerResponse>> UpdateExistingCustomer([FromBody] CustomerUpsertRequest customerToBeUpdated, int customerId)
    {
        var updatedCustomer = await _customerService.UpdateCustomer(customerToBeUpdated.MapToCustomerDto(customerId));
        return Ok(updatedCustomer.MapToCustomerResponse());
    }
}