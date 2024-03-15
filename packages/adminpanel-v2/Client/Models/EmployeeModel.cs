namespace Alvtime.Adminpanel.Client.Models;

public class EmployeeModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int EmployeeId { get; set; }
    public bool ShowDetails { get; set; }
    public IList<EmployeeEmploymentRateModel>? EmploymentRates { get; set; }
}

public class EmployeeEmploymentRateModel
{
    public int Id { get; set; }
    public decimal RatePercentage { get; set; }
    public DateTime? FromDateInclusive { get; set; }
    public DateTime? ToDateInclusive { get; set; }
}