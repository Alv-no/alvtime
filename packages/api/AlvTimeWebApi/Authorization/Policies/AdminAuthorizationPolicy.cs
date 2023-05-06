using Microsoft.AspNetCore.Authorization;
using System;
using AlvTimeWebApi.Authorization.Requirements;

namespace AlvTimeWebApi.Authorization.Policies
{
    public class AdminAuthorizationPolicy
    {
        public static string Name => "Admin";

        public static void Build(AuthorizationPolicyBuilder builder) => builder
                .RequireAuthenticatedUser()
                .RequireClaim("groups", Environment.GetEnvironmentVariable("AdminGroup"))
                .AddRequirements(new EmployeeStillActiveRequirement());
    }
}
