using System.Linq;
using AlvTime.Business.Customers;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Responses.Admin;
using AlvTimeWebApi.Utils;

namespace AlvTimeWebApi.Controllers.Utils;

public static class CustomerMapper
{
    public static CustomerAdminResponse MapToCustomerResponse(this CustomerAdminDto customer)
    {
        return new CustomerAdminResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            InvoiceAddress = customer.InvoiceAddress,
            ContactPerson = customer.ContactPerson,
            ContactEmail = customer.ContactEmail,
            ContactPhone = customer.ContactPhone,
            Projects = customer.Projects.Select(p => new ProjectAdminResponse
            {
                Id = p.Id,
                Name = p.Name,
                Tasks = p.Tasks.Select(t => new TaskAdminResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Locked = t.Locked,
                    Imposed = t.Imposed,
                    CompensationRate = t.CompensationRate,
                    HourRates = t.HourRates.Select(hr => new HourRateAdminResponse
                    {
                        Id = hr.Id,
                        FromDate = hr.FromDate.ToDateOnly(),
                        Rate = 0
                    })
                })
            })
        };
    }
}