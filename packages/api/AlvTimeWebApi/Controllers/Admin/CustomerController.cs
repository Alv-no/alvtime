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
        private readonly ICustomerStorage _storage;
        private readonly CustomerCreator _creator;

        public CustomerController(ICustomerStorage storage, CustomerCreator creator)
        {
            _storage = storage;
            _creator = creator;
        }

        [HttpGet("Customers")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CustomerDto>> FetchCustomers()
        {
            return Ok(_storage.GetCustomers(new CustomerQuerySearch()));
        }

        [HttpPost("Customers")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CustomerDto>> CreateNewCustomers([FromBody] IEnumerable<CustomerDto> customersToBeCreated)
        {
            List<CustomerDto> response = new List<CustomerDto>();

            foreach (var customer in customersToBeCreated)
            {
                response.Add(_creator.CreateCustomer(customer));
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
                response.Add(_creator.UpdateCustomer(customer));
            }
            return Ok(response);
        }
    }
}
