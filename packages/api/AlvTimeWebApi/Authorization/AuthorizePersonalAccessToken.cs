using AlvTimeWebApi.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Authorization;

public class AuthorizePersonalAccessToken : AuthorizeAttribute
{
    public AuthorizePersonalAccessToken() : base(AllowPersonalAccessTokenPolicy.Name)
    {

    }
}