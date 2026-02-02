using System.Net;
using System.Threading.Tasks;
using AlvTimeWebApi.Authentication.OAuth;
using AlvTimeWebApi.Authentication.PersonalAccessToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlvTimeWebApi.Authentication;

public static class AuthenticationExtensions
{
    public static void AddAlvtimeAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authentication = new OAuthOptions();
        configuration.Bind("AzureAd", authentication);

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "AzureAd";
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect("AzureAd", options =>
            {
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = _ => Task.CompletedTask,
                    OnTokenValidated = _ => Task.CompletedTask
                };
                options.Authority = $"{authentication.Instance}{authentication.TenantId}";
                options.ClientId = authentication.ClientId;
                options.ClientSecret = authentication.AuthCodeFlowSecret;
                options.ResponseType = "code";
                options.CallbackPath = "/signin-oidc";
                options.UsePkce = true;
            })
            // .AddJwtBearer(options =>
            // {
            //     options.Authority = $"{authentication.Instance}{authentication.TenantId}";
            //     options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            //     {
            //         ValidAudience = $"{authentication.ClientId}",
            //         ValidIssuer = $"{authentication.Instance}{authentication.TenantId}/v2.0"
            //     };
            // })
            .AddScheme<PersonalAccessTokenOptions, PersonalAccessTokenHandler>("PersonalAccessTokenScheme", null);
    }
}