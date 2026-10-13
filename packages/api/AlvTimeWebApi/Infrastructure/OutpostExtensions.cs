using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AlvTimeWebApi.Infrastructure
{
    public static class OutpostExtensions
    {
        public static void AddOutpostClient(this IServiceCollection services, IConfiguration configuration)
        {
            // Without an api key webhooks are skipped (see WebhookOutboxProcessor), might wanna throw exception if api key is missing 
            services.AddHttpClient("WebhookOutpost", c =>
            {
                c.BaseAddress = new Uri(configuration["Outpost:BaseUrl"]!);
                c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", configuration["Outpost:ApiKey"]);

                c.Timeout = TimeSpan.FromSeconds(15);
            });
        }
    }
}
