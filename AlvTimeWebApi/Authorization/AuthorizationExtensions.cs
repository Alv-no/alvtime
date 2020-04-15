using AlvTimeWebApi.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AlvTimeWebApi.Authorization
{
    public static class AuthorizationExtensions
    {
        public static void AddAlvtimeAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AdminAuthorizationPolicy.Name, AdminAuthorizationPolicy.Build);
                options.AddPolicy(AllowPersonalAccessTokenPolicy.Name, AllowPersonalAccessTokenPolicy.Build);
            });
        }
    }
}
