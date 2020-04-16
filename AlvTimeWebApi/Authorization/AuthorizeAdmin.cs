using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Authentication
{
    public class AuthorizeAdmin : AuthorizeAttribute
    {
        public AuthorizeAdmin() : base(AdminAuthorizationPolicy.Name)
        {

        }
    }
}
