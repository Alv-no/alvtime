using AlvTimeWebApi.Authorization.Handlers;
using AlvTimeWebApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AlvTimeWebApi.Authorization;

public static class AuthorizationExtensions
{
    public static void AddAlvtimeAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("PersonalAccessTokenScheme", CookieAuthenticationDefaults.AuthenticationScheme)
                .AddRequirements(new EmployeeStillActiveRequirement())
                .Build();
        });
        services.AddScoped<IAuthorizationHandler, EmployeeIsActiveHandler>();
    }
}