using System.Linq;
using AlvTime.Business.Customers;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Responses.Admin;
using AlvTimeWebApi.Utils;

namespace AlvTimeWebApi.Controllers.Utils;

public static class CustomerMapper
{
    public static CustomerDetailedResponse MapToCustomerResponse(this CustomerAdminDto customer)
    {
        return new CustomerDetailedResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            InvoiceAddress = customer.InvoiceAddress,
            ContactPerson = customer.ContactPerson,
            ContactEmail = customer.ContactEmail,
            ContactPhone = customer.ContactPhone,
            OrgNr = customer.OrgNr,
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
                        Rate = hr.Rate
                    })
                })
            })
        };
    }

    public static CustomerDto MapToCustomerDto(this CustomerUpsertRequest customer, int? id)
    {
        return new CustomerDto
        {
            Id = id,
            Name = customer.Name,
            InvoiceAddress = customer.InvoiceAddress,
            ContactPerson = customer.ContactPerson,
            ContactEmail = customer.ContactEmail,
            ContactPhone = customer.ContactPhone,
            OrgNr = customer.OrgNr
        };
    }

    public static CustomerResponse MapToCustomerResponse(this CustomerDto customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name,
            InvoiceAddress = customer.InvoiceAddress,
            ContactPerson = customer.ContactPerson,
            ContactEmail = customer.ContactEmail,
            ContactPhone = customer.ContactPhone,
            OrgNr = customer.OrgNr
        };
    }
}