using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlvTimeWebApi.Logging;

public static class LoggingExtensions
{
    public static void ConfigureLogging(this IServiceCollection services)
    {
        services.AddOpenTelemetry().UseAzureMonitor();
    }
}