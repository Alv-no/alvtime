using AlvTime.Business.Customers;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class CustomerController : Controller
    {
        private readonly CustomerService _customerService;

        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("Customers")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CustomerDto>> FetchCustomers()
        {
            return Ok(_customerService.GetCustomers(new CustomerQuerySearch()));
        }

        [HttpPost("Customers")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CustomerDto>> CreateNewCustomers([FromBody] IEnumerable<CustomerDto> customersToBeCreated)
        {
            List<CustomerDto> response = new List<CustomerDto>();

            foreach (var customer in customersToBeCreated)
            {
                response.Add(_customerService.CreateCustomer(customer));
            }

            return Ok(response);
        }

        [HttpPut("Customers")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CustomerDto>> UpdateExistingCustomers([FromBody] IEnumerable<CustomerDto> customersToBeUpdated)
        {
            List<CustomerDto> response = new List<CustomerDto>();

            foreach (var customer in customersToBeUpdated)
            {
                response.Add(_customerService.UpdateCustomer(customer));
            }
            return Ok(response);
        }
    }
}
