using System.Collections.Generic;
using AlvTime.Business.Projects;

namespace AlvTime.Business.Customers;

public class CustomerAdminDto
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public string InvoiceAddress { get; set; }
    public string ContactPerson { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public IEnumerable<ProjectAdminDto> Projects { get; set; }
}