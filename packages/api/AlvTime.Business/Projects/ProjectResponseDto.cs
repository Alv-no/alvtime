using AlvTime.Business.Customers;

namespace AlvTime.Business.Projects;

public class ProjectResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public CustomerDto Customer { get; set; }
}