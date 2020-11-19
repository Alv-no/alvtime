using AlvTimeWebApi.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Authorization
{
    public class AuthorizeReporting : AuthorizeAttribute
    {
        public AuthorizeReporting() : base(ReportAuthorizationPolicy.Name)
        {

        }
    }
}
