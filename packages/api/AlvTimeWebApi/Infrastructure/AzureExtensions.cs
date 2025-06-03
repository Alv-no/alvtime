using System;
using System.Threading.Tasks;
using AlvTimeWebApi.Authentication.OAuth;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;

namespace AlvTimeWebApi.Infrastructure;

public static class AzureExtensions
{
    public static void AddMicrosoftGraphClient(this IServiceCollection services, IConfiguration configuration)
    {
        var authentication = new OAuthOptions();
        configuration.Bind("AzureAd", authentication);

        var scopes = new[] { "https://graph.microsoft.com/.default" };

        var tenantId = authentication.TenantId;
        var clientId = authentication.ClientId;
        var clientSecret = authentication.GraphClientSecret;
        Console.WriteLine("ClientSecret: " + clientSecret);

        var options = new ClientSecretCredential(tenantId, clientId, clientSecret);
        var graphClient = new GraphServiceClient(options, scopes);
        services.AddSingleton(graphClient);
    }
}