using Microsoft.AspNetCore.Authorization;
using System;
using AlvTimeWebApi.Authorization.Requirements;

namespace AlvTimeWebApi.Authorization.Policies
{
    public class OrakeletAuthorizationPolicy
    {
        public static string Name => "Orakelet";

        public static void Build(AuthorizationPolicyBuilder builder) => builder
                .RequireAuthenticatedUser()
                .RequireClaim("groups", Environment.GetEnvironmentVariable("OrakeletGroup")) ;
    }
}
