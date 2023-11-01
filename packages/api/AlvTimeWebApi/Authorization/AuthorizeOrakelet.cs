using AlvTimeWebApi.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Authentication;

public class AuthorizeOrakelet : AuthorizeAttribute
{
    public AuthorizeOrakelet() : base(OrakeletAuthorizationPolicy.Name)
    {

    }
}
