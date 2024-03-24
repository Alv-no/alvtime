using Alvtime.Adminpanel.Client.Models;
using Alvtime.Adminpanel.Client.Requests;

namespace Alvtime.Adminpanel.Client.Mappers;

public static class CustomerMapper
{
    public static CustomerUpsertRequest MapToCustomerUpsertRequest(this CustomerModel customer)
    {
        return new CustomerUpsertRequest
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