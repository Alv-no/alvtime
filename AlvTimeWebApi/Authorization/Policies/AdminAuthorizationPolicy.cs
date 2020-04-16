using Microsoft.AspNetCore.Authorization;
using System;

namespace AlvTimeWebApi.Authentication
{
    public class AdminAuthorizationPolicy
    {
        public static string Name => "Admin";

        public static void Build(AuthorizationPolicyBuilder builder) =>
            builder.RequireClaim("groups", Environment.GetEnvironmentVariable("AdminGroup"));
    }
}
