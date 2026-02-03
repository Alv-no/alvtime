using System.Collections.Generic;

namespace AlvTimeWebApi.Responses;

public class UserInfo
{
    public bool IsAuthenticated { get; set; }
    public string Name { get; set; }
    public IEnumerable<string> Roles { get; set; }
}