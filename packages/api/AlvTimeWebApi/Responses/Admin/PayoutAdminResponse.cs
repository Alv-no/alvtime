namespace AlvTimeWebApi.Responses.Admin;

public class PayoutAdminResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Date { get; set; }
    public decimal HoursBeforeCompRate { get; set; }
    public decimal HoursAfterCompRate { get; set; }
    public bool Active { get; set; }
    public decimal CompensationRate { get; set; }
}
