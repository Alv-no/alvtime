using System;
using System.Net;
using System.Threading.Tasks;
using AlvTimeWebApi.Authentication.OAuth;
using AlvTimeWebApi.Authentication.PersonalAccessToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AlvTimeWebApi.Authentication;

public static class AuthenticationExtensions
{
    public static void AddAlvtimeAuthentication(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment env)
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
                options.Cookie.Name = "AlvTimeAuthorizationCookie";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect("AzureAd", options =>
            {
                if (!env.IsDevelopment())
                {
                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        var builder = new UriBuilder(context.ProtocolMessage.RedirectUri)
                        {
                            Scheme = "https",
                        };
                        context.ProtocolMessage.RedirectUri = builder.ToString();
                        return Task.CompletedTask;
                    };
                }

                options.Events.OnTokenValidated = _ => Task.CompletedTask;
                options.Authority = $"{authentication.Instance}{authentication.TenantId}";
                options.ClientId = authentication.ClientId;
                options.ClientSecret = authentication.AuthCodeFlowSecret;
                options.ResponseType = "code";
                options.CallbackPath = "/signin-oidc";
                options.UsePkce = true;
            })
            .AddScheme<PersonalAccessTokenOptions, PersonalAccessTokenHandler>("PersonalAccessTokenScheme", null);
    }
}