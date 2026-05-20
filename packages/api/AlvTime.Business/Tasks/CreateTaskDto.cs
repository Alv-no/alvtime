namespace AlvTime.Business.Tasks;

public class TaskDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Locked { get; set; }
    public decimal CompensationRate { get; set; }
    public bool Imposed { get; set; }
    public CompensationType CompensationType { get; set; }
}