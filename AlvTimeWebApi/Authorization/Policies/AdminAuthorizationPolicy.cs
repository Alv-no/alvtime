using Microsoft.AspNetCore.Authorization;
using System;

namespace AlvTimeWebApi.Authorization.Policies
{
    public class AdminAuthorizationPolicy
    {
        public static string Name => "Admin";

        public static void Build(AuthorizationPolicyBuilder builder) =>
            builder.RequireClaim("groups", "5850c192-c101-4b64-9c45-cbbf73542805");
    }
}
