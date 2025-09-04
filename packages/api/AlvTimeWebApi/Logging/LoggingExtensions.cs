using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AlvTimeWebApi.Logging;

public static class LoggingExtensions
{
    public static void ConfigureLogging(this IServiceCollection services, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddOpenTelemetry();
            return;
        }
        services.AddOpenTelemetry().UseAzureMonitor();
    }
}