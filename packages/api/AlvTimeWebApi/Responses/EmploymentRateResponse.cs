namespace AlvTimeWebApi.Responses;

public class EmploymentRateResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Rate { get; set; }
    public string FromDateInclusive { get; set; }
    public string ToDateInclusive { get; set; }
}