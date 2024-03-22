namespace AlvTimeWebApi.Responses.Admin;

public class CustomerResponse
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public string InvoiceAddress { get; set; }
    public string ContactPerson { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public string OrgNr { get; set; }
}