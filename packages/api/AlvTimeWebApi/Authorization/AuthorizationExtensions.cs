using AlvTimeWebApi.Authorization.Policies;
using Microsoft.Extensions.DependencyInjection;
using AlvTimeWebApi.Authorization.Handlers;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Authorization;

public static class AuthorizationExtensions
{
    public static void AddAlvtimeAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AdminAuthorizationPolicy.Name, AdminAuthorizationPolicy.Build);
            options.AddPolicy(OrakeletAuthorizationPolicy.Name, OrakeletAuthorizationPolicy.Build);
            options.AddPolicy(AllowPersonalAccessTokenPolicy.Name, AllowPersonalAccessTokenPolicy.Build);
            options.AddPolicy(ReportAuthorizationPolicy.Name, ReportAuthorizationPolicy.Build);
        });
        services.AddScoped<IAuthorizationHandler, EmployeeIsActiveHandler>();
    }
}