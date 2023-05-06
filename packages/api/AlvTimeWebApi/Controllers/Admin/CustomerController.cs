using AlvTime.Business.Customers;
using AlvTimeWebApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    [AuthorizeAdmin]
    public class CustomerController : Controller
    {
        private readonly ICustomerStorage _customerStorage;
        private readonly CustomerService _customerService;

        public CustomerController(ICustomerStorage customerStorage, CustomerService customerService)
        {
            _customerStorage = customerStorage;
            _customerService = customerService;
        }

        [HttpGet("Customers")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> FetchCustomers()
        {
            return Ok(await _customerStorage.GetCustomers(new CustomerQuerySearch()));
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
}
