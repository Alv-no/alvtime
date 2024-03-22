using Alvtime.Adminpanel.Client.Models;
using Alvtime.Adminpanel.Client.Requests;

namespace Alvtime.Adminpanel.Client.Mappers;

public static class CustomerMapper
{
    public static CustomerUpdateRequest MapToCustomerUpdateRequest(this CustomerModel customer)
    {
        return new CustomerUpdateRequest
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
    
    public static CustomerCreateRequest MapToCustomerCreateRequest(this CustomerModel customer)
    {
        return new CustomerCreateRequest
        {
            Name = customer.Name,
            InvoiceAddress = customer.InvoiceAddress,
            ContactPerson = customer.ContactPerson,
            ContactEmail = customer.ContactEmail,
            ContactPhone = customer.ContactPhone,
            OrgNr = customer.OrgNr
        };
    }
}