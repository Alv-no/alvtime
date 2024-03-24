namespace Alvtime.Adminpanel.Client.Requests;

public class EmploymentRateUpsertRequest
{
    public decimal RatePercentage { get; set; }
    public DateTime? FromDateInclusive { get; set; }
    public DateTime? ToDateInclusive { get; set; }
}