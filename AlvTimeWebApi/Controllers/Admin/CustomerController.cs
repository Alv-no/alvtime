using AlvTime.Business.Tasks;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.HelperClasses;
using AlvTimeWebApi.Persistence.DatabaseModels;
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
                    Name = x.Name,
                    ContactPerson = x.ContactPerson,
                    ContactEmail = x.ContactEmail,
                    ContactPhone = x.ContactPhone,
                    InvoiceAddress = x.InvoiceAddress
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
                        Name = customer.Name,
                        InvoiceAddress = customer.InvoiceAddress != null ? customer.InvoiceAddress : "",
                        ContactPhone = customer.ContactPhone != null ? customer.ContactPhone : "",
                        ContactEmail = customer.ContactEmail != null ? customer.ContactEmail : "",
                        ContactPerson = customer.ContactPerson != null ? customer.ContactPerson : ""
                    };
                    _database.Customer.Add(newCustomer);
                    _database.SaveChanges();

                    response.Add(returnObjects.ReturnCustomer(customer.Name));
                }
            }
            return Ok(response);
        }

        [HttpPost("UpdateCustomer")]
        [AuthorizeAdmin]
        public ActionResult<IEnumerable<CustomerDto>> UpdateExistingCustomer([FromBody] IEnumerable<UpdateCustomerDto> customersToBeUpdated)
        {
            List<CustomerDto> response = new List<CustomerDto>();

            foreach (var customer in customersToBeUpdated)
            {
                var existingCustomer = _database.Customer.FirstOrDefault(x => x.Id == customer.Id);

                if(customer.ContactEmail != null)
                {
                    existingCustomer.ContactEmail = customer.ContactEmail;
                }
                if(customer.ContactPerson != null)
                {
                    existingCustomer.ContactPerson = customer.ContactPerson;
                }
                if(customer.ContactPhone != null)
                {
                    existingCustomer.ContactPhone = customer.ContactPhone;
                }
                if(customer.InvoiceAddress != null){
                    existingCustomer.InvoiceAddress = customer.InvoiceAddress;
                }
                
                _database.SaveChanges();

                response.Add(returnObjects.ReturnCustomer(existingCustomer.Name));
            }
            return Ok(response);
        }
    }
}
