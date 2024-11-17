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
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();            
        });
        services.AddScoped<IAuthorizationHandler, EmployeeIsActiveHandler>();
    }
}