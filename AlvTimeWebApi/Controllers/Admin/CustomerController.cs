using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.DatabaseModels;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    public class CustomerController : Controller
    {
        private readonly AlvTime_dbContext _database;

        private CreatedObjectReturner returnObjects;
        private ExistingObjectFinder checkExisting;

        public CustomerController(AlvTime_dbContext database)
        {
            _database = database;
            returnObjects = new CreatedObjectReturner(_database);
            checkExisting = new ExistingObjectFinder(_database);
        }

        [HttpGet("Customers")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CustomerDto>> CreateNewCustomer()
        {
            var customers = _database.Customer
                .Select(x => new CustomerDto
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList();

            return Ok(customers);
        }

        [HttpPost("CreateCustomer")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CustomerDto>> CreateNewCustomer([FromBody] IEnumerable<CreateCustomerDto> customersToBeCreated)
        {
            List<CustomerDto> response = new List<CustomerDto>();

            foreach (var customer in customersToBeCreated)
            {
                if (checkExisting.CustomerDoesNotExist(customer))
                {
                    var newCustomer = new Customer
                    {
                        Name = customer.Name
                    };
                    _database.Customer.Add(newCustomer);
                    _database.SaveChanges();

                    response.Add(returnObjects.ReturnCreatedCustomer(customer));
                }
            }
            return Ok(response);
        }
    }
}
