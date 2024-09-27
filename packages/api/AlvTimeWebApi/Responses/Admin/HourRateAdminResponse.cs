namespace AlvTimeWebApi.Responses.Admin;

public class HourRateAdminResponse
{
    public int Id { get; set; }
    public string FromDate { get; set; }
    public decimal Rate { get; set; }
    public int TaskId { get; set; }
    public string TaskName { get; set; }
}