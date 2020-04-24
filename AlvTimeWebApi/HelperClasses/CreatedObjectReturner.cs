using AlvTime.Business.HourRates;
using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTimeWebApi.Dto;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System.Globalization;
using System.Linq;

namespace AlvTimeWebApi.HelperClasses
{
    public class CreatedObjectReturner
    {
        private readonly AlvTime_dbContext _database;

        public CreatedObjectReturner(AlvTime_dbContext database)
        {
            _database = database;
        }

        public CustomerDto ReturnCustomer(string customerName)
        {
            var customerResponseDto = _database.Customer
                .Where(x => x.Name == customerName)
                .Select(x => new CustomerDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ContactPerson = x.ContactPerson,
                    ContactEmail = x.ContactEmail,
                    ContactPhone = x.ContactPhone,
                    InvoiceAddress = x.InvoiceAddress
                })
                .FirstOrDefault();

            return customerResponseDto;
        }

        public ProjectResponseDto ReturnCreatedProject(CreateProjectDto project)
        {
            var projectResponseDto = _database.Project
                .Where(x => x.Name == project.Name && x.Customer == project.Customer)
                .Select(x => new ProjectResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Customer = new CustomerDto
                    {
                        Id = x.CustomerNavigation.Id,
                        Name = x.CustomerNavigation.Name,
                        ContactEmail = x.CustomerNavigation.ContactEmail,
                        ContactPerson = x.CustomerNavigation.ContactPerson,
                        ContactPhone = x.CustomerNavigation.ContactPhone,
                        InvoiceAddress = x.CustomerNavigation.InvoiceAddress
                    }
                })
                .FirstOrDefault();

            return projectResponseDto;
        }
    }
}
