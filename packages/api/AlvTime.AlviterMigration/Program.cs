using AlvTime.AlviterMigration.Services;
using AlvTime.Common.Configuration;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.CommonConfigure<Program>();
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("AlvTime")
            ?? throw new InvalidOperationException("Connection string 'AlvTime' is not configured.");

        services.AddDbContext<AlvTime_dbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddTransient<ICsvReader, CsvReader>();
        services.AddTransient<IDatabaseReader, DatabaseReader>();
        services.AddTransient<IMigrationCalculator, MigrationCalculator>();
        services.AddTransient<IDatabaseWriter, DatabaseWriter>();
        services.AddTransient<IMigrationOrchestrator, MigrationOrchestrator>();
    })
    .Build();

using var scope = host.Services.CreateScope();
var orchestrator = scope.ServiceProvider.GetRequiredService<IMigrationOrchestrator>();
var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

var csvFilePath = configuration["Migration:CsvFilePath"] ?? "Alviter_timer_2025.csv";
await orchestrator.RunAsync(csvFilePath);
