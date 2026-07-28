using System.Collections.Generic;
using AlvTime.Business.Customers;
using AlvTime.Business.Tasks;

namespace AlvTime.Business.Projects;

public class ProjectResponseDtoV2
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Index { get; set; }
    public IEnumerable<TaskResponseDtoV2> Tasks { get; set; }
    public CustomerResponseDto Customer { get; set; }
}