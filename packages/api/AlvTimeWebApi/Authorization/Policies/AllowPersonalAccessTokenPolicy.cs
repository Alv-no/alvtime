using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Authorization
{
    internal class AllowPersonalAccessTokenPolicy
    {
        public const string Name = "AllowPersonalAccessToken";

        internal static void Build(AuthorizationPolicyBuilder builder) => builder
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes("PersonalAccessTokenScheme", JwtBearerDefaults.AuthenticationScheme);
    }
}
