namespace AlvTimeWebApi.Responses.Admin;

public class TaskResponseSimple
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Favorite { get; set; }
    public bool Locked { get; set; }
    public decimal CompensationRate { get; set; }
}