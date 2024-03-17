using System.Collections.Generic;
using AlvTime.Business.Tasks;

namespace AlvTime.Business.Projects;

public class ProjectAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<TaskAdminDto> Tasks { get; set; }
}