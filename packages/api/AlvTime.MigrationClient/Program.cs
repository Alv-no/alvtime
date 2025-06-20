using AlvTime.Common.Configuration;
using AlvTime.MigrationClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.CommonConfigure<Program>();
    })
    .ConfigureServices((_, services) =>
    {
        services.AddTransient<IMigrationService, MigrationService>();
    })
    .Build();

using var scope = host.Services.CreateScope();
var serviceProvider = scope.ServiceProvider;
var migrationService = serviceProvider.GetRequiredService<IMigrationService>();
var configuration = serviceProvider.GetRequiredService<IConfiguration>();
var connectionString = configuration.GetConnectionString("AlvTime");
var env = serviceProvider.GetRequiredService<IHostEnvironment>();
await migrationService.RunMigrations(connectionString ?? throw new InvalidOperationException(), shouldSeed: env.IsDevelopment());