using AlvTime.Business.Customers;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Projects;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.ErrorHandling;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses.Admin;

namespace AlvTimeWebApi.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[AuthorizeAdmin]
public class CustomerController : ControllerBase
{
    private readonly CustomerService _customerService;
    private readonly ProjectService _projectService;

    public CustomerController(CustomerService customerService, ProjectService projectService)
    {
        _customerService = customerService;
        _projectService = projectService;
    }

    [HttpGet("Customers")]    
    public async Task<ActionResult<IEnumerable<CustomerAdminResponse>>> FetchCustomersAdmin()
    {
        var customers = await _customerService.GetCustomersAdmin();
        return Ok(customers.Select(c => c.MapToCustomerAdminResponse()));
    }

    [HttpPost("Customers")]
    public async Task<ActionResult<CustomerResponse>> CreateNewCustomer([FromBody] CustomerUpsertRequest customerToBeCreated)
    {
        var result = await _customerService.CreateCustomer(customerToBeCreated.MapToCustomerDto(null));
        return result.Match<ActionResult<CustomerResponse>>(
            customer => Ok(customer.MapToCustomerResponse()),
            errors => BadRequest(errors.ToValidationProblemDetails("Opprettelse av kunde feilet")));
    }

    [HttpGet("Customers/{customerId:int}")] 
    public async Task<ActionResult<CustomerResponse>> GetCustomerById(int customerId)
    {
        var result = await _customerService.GetCustomerDetailedById(customerId);
        return result.Match<ActionResult<CustomerResponse>>(
            customer => customer != null 
                ? Ok(CustomerMapper.MapToCustomerResponse(customer))
                : NotFound(),
            errors => BadRequest(errors.ToValidationProblemDetails("Henting av kunde feilet")));
    }
    
    [HttpPut("Customers/{customerId:int}")]
    public async Task<ActionResult<CustomerResponse>> UpdateExistingCustomer([FromBody] CustomerUpsertRequest customerToBeUpdated, int customerId)
    {
        var result = await _customerService.UpdateCustomer(customerToBeUpdated.MapToCustomerDto(customerId));
        return result.Match<ActionResult<CustomerResponse>>(
            customer => Ok(customer.MapToCustomerResponse()),
            errors => BadRequest(errors.ToValidationProblemDetails("Oppdatering av kunde feilet")));
    }
}