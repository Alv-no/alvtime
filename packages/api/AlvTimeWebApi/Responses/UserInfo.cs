using System.Collections.Generic;
using System.Security.Claims;

namespace AlvTimeWebApi.Responses;

public class UserInfo
{
    public bool IsAuthenticated { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public IEnumerable<Claim> Claims { get; set; }
}