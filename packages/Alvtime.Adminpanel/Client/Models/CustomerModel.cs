namespace Alvtime.Adminpanel.Client.Models;

public class CustomerModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? InvoiceAddress { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? OrgNr { get; set; }
    public IList<ProjectModel>? Projects { get; set; }
    public bool ShowDetails { get; set; }
}

public class ProjectModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public IList<TaskModel>? Tasks { get; set; }
    public bool ShowDetails { get; set; }
}

public class TaskModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool Locked { get; set; }
    public bool Imposed { get; set; }
    public decimal CompensationRate { get; set; }
    public IList<HourRateModel>? HourRates { get; set; }
    public bool ShowDetails { get; set; }
}

public class HourRateModel
{
    public int Id { get; set; }
    public DateTime? FromDate { get; set; }
    public decimal Rate { get; set; }
}