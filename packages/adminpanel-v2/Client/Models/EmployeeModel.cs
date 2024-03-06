namespace Alvtime.Adminpanel.Client.Models;

public class EmployeeModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string StartDate { get; set; }
    public string? EndDate { get; set; }
    public int EmployeeId { get; set; }
    public IEnumerable<EmployeeEmploymentRateModel>? EmploymentRates { get; set; }

}

public class EmployeeEmploymentRateModel
{
    public int Id { get; set; }
    public decimal RatePercentage { get; set; }
    public string FromDateInclusive { get; set; }
    public string ToDateInclusive { get; set; }
}