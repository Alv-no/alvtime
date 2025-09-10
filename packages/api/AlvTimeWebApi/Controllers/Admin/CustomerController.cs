using AlvTime.Business.Customers;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.ErrorHandling;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses.Admin;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class CustomerController(CustomerService customerService) : ControllerBase
{
    [HttpGet("Customers")]    
    public async Task<ActionResult<IEnumerable<CustomerAdminResponse>>> FetchCustomersAdmin()
    {
        var customers = await customerService.GetCustomersAdmin();
        return Ok(customers.Select(c => c.MapToCustomerResponse()));
    }

    [HttpPost("Customers")]
    public async Task<ActionResult<CustomerResponse>> CreateNewCustomer([FromBody] CustomerUpsertRequest customerToBeCreated)
    {
        var result = await customerService.CreateCustomer(customerToBeCreated.MapToCustomerDto(null));
        return result.Match<ActionResult<CustomerResponse>>(
            customer => Ok(customer.MapToCustomerResponse()),
            errors => BadRequest(errors.ToValidationProblemDetails("Opprettelse av kunde feilet")));
    }

    [HttpGet("Customers/{customerId:int}")] 
    public async Task<ActionResult<CustomerAdminResponse>> GetCustomerById(int customerId)
    {
        var result = await customerService.GetCustomerDetailedById(customerId);
        return result.Match<ActionResult<CustomerAdminResponse>>(
            customer => customer != null 
                ? Ok(customer.MapToCustomerResponse())
                : NotFound(),
            errors => BadRequest(errors.ToValidationProblemDetails("Henting av kunde feilet")));
    }
    
    [HttpPut("Customers/{customerId:int}")]
    public async Task<ActionResult<CustomerResponse>> UpdateExistingCustomer([FromBody] CustomerUpsertRequest customerToBeUpdated, int customerId)
    {
        var result = await customerService.UpdateCustomer(customerToBeUpdated.MapToCustomerDto(customerId));
        return result.Match<ActionResult<CustomerResponse>>(
            customer => Ok(customer.MapToCustomerResponse()),
            errors => BadRequest(errors.ToValidationProblemDetails("Oppdatering av kunde feilet")));
    }
}