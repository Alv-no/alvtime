namespace AlvTimeWebApi.Responses.Admin;

public class HourRateResponse
{
    public string FromDate { get; set; }
    public decimal Rate { get; set; }
    public int TaskId { get; set; }
    public int Id { get; set; }
}