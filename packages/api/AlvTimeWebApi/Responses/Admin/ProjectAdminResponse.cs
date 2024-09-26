using System.Collections.Generic;

namespace AlvTimeWebApi.Responses.Admin;

public class ProjectAdminResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TaskCount { get; set; }
    public IEnumerable<TaskAdminResponse> Tasks { get; set; }
}