namespace Alvtime.Adminpanel.Client.Requests;

public class HourRateUpsertRequest
{
    public DateTime FromDate { get; set; }
    public decimal Rate { get; set; }
}