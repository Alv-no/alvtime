using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlvTimeWebApi.Authentication
{
    public static class AuthenticationExtensions
    {
        public static void AddAlvtimeAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var authentication = new AuthenticationOptions();
            configuration.Bind("AzureAd", authentication);

            services
                .AddAuthorization(options =>
                {
                    options.AddPolicy(AdminAuthorizationPolicy.Name, AdminAuthorizationPolicy.Build);
                })
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = $"{authentication.Instance}{authentication.TenantId}";
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidAudience = $"{authentication.ClientId}",
                        ValidIssuer = $"{authentication.Instance}{authentication.TenantId}/v2.0"
                    };
                });
        }
    }
}
