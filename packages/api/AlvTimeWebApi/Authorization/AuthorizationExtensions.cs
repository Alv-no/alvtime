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
        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("PersonalAccessTokenScheme", CookieAuthenticationDefaults.AuthenticationScheme)
                .AddRequirements(new EmployeeStillActiveRequirement())
                .Build())
            .AddPolicy("AdminPolicy", policy =>
            {
                policy.AddAuthenticationSchemes(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    "Alviter");
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context => context.User.IsInRole("Admin"));
            });
        services.AddScoped<IAuthorizationHandler, EmployeeIsActiveHandler>();
    }
}