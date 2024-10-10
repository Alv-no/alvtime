using System.Collections.Generic;

namespace AlvTimeWebApi.Responses.Admin;

public class CustomerDetailedResponse
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public string InvoiceAddress { get; set; }
    public string ContactPerson { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public string OrgNr { get; set; }
    
    public int ProjectCount { get; set; }
    public IEnumerable<ProjectAdminResponse> Projects { get; set; }
}