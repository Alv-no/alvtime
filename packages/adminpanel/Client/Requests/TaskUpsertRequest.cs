namespace Alvtime.Adminpanel.Client.Requests;

public class TaskUpsertRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool Locked { get; set; }
    public decimal CompensationRate { get; set; }
    public bool Imposed { get; set; }
}