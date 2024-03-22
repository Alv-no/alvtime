namespace Alvtime.Adminpanel.Client.Requests;

public class EmployeeCreateRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int EmployeeId { get; set; }
}