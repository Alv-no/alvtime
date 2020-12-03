using Microsoft.AspNetCore.Authorization;
using System;

namespace AlvTimeWebApi.Authorization.Policies
{
    public class ReportAuthorizationPolicy
    {
        public static string Name => "Reporting";

        public static void Build(AuthorizationPolicyBuilder builder) =>
            builder.RequireClaim("groups", Environment.GetEnvironmentVariable("ReportGroup"));
    }
}
