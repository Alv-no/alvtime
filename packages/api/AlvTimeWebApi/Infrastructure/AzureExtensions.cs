using AlvTimeWebApi.Authentication.OAuth;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;

namespace AlvTimeWebApi.Infrastructure;

public static class AzureExtensions
{
    public static void AddMicrosoftGraphClient(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var authentication = new OAuthOptions();
        configuration.Bind("AzureAd", authentication);

        var scopes = new[] { "https://graph.microsoft.com/.default" };

        if (environment.IsDevelopment())
        {
            var tenantId = authentication.TenantId;
            var clientId = authentication.ClientId;
            var clientSecret = authentication.GraphClientSecret;

            var options = new ClientSecretCredential(tenantId, clientId, clientSecret);
            services.AddSingleton(new GraphServiceClient(options, scopes));
        }
        else
        {
            var credential = new DefaultAzureCredential();
            services.AddSingleton(new GraphServiceClient(credential, scopes));
        }
    }
}