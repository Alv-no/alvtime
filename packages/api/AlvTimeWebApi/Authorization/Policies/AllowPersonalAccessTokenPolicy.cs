using AlvTimeWebApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Authorization.Policies
{
    internal class AllowPersonalAccessTokenPolicy
    {
        public const string Name = "AllowPersonalAccessToken";

        internal static void Build(AuthorizationPolicyBuilder builder) => builder
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes("PersonalAccessTokenScheme", JwtBearerDefaults.AuthenticationScheme)
            .AddRequirements(new EmployeeStillActiveRequirement());
    }
}
