namespace AlvTime.Business.Projects;

public class CreateProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? Customer { get; set; }
}