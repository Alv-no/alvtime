using System.Linq;
using AlvTime.Business.Customers;
using AlvTimeWebApi.Requests;
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
            OrgNr = customer.OrgNr,
            ProjectCount = customer.ProjectCount,
            Projects = customer.Projects.Select(p => new ProjectAdminResponse
            {
                Id = p.Id,
                Name = p.Name,
                TaskCount = p.TaskCount,
                Tasks = p.Tasks.Select(t => new TaskAdminResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Locked = t.Locked,
                    Imposed = t.Imposed,
                    CompensationRate = t.CompensationRate,
                    ProjectId = t.ProjectId,
                    ProjectName = t.ProjectName,
                    HourRates = t.HourRates.Select(hr => new HourRateAdminResponse
                    {
                        Id = hr.Id,
                        FromDate = hr.FromDate.ToDateOnly(),
                        Rate = hr.Rate,
                        TaskId = t.Id,
                        TaskName = t.Name
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