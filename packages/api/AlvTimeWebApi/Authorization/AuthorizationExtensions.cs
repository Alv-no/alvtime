using Microsoft.Extensions.DependencyInjection;
using AlvTimeWebApi.Authorization.Handlers;
using AlvTimeWebApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace AlvTimeWebApi.Authorization;

public static class AuthorizationExtensions
{
    public static void AddAlvtimeAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("PersonalAccessTokenScheme", JwtBearerDefaults.AuthenticationScheme)
                .AddRequirements(new EmployeeStillActiveRequirement())
                .Build();
        });
        services.AddScoped<IAuthorizationHandler, EmployeeIsActiveHandler>();
    }
}