using AlvTime.Common.Configuration;
using AlvTime.MigrationClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.SqlServer;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.CommonConfigure<Program>();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddTransient<IMigrationService, MigrationService>();

        // Only used to let Wolverine create/update its own message storage, the host is never started
        services.AddWolverine(opts =>
        {
            opts.PersistMessagesWithSqlServer(context.Configuration.GetConnectionString("AlvTime")!, "wolverine");
        });
    })
    .Build();

using var scope = host.Services.CreateScope();
var serviceProvider = scope.ServiceProvider;
var migrationService = serviceProvider.GetRequiredService<IMigrationService>();
var configuration = serviceProvider.GetRequiredService<IConfiguration>();
var connectionString = configuration.GetConnectionString("AlvTime");
var env = serviceProvider.GetRequiredService<IHostEnvironment>();
await migrationService.RunMigrations(connectionString ?? throw new InvalidOperationException(), shouldSeed: env.IsDevelopment());