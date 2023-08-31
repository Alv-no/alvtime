using System;
using System.Threading.Tasks;
using AlvTime.MigrationClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AlvTimeWebApi
{
    public class Program
    {
        private readonly IConfiguration _configuration;

        public Program(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var environment = builder.Environment;
            var connectionString = builder.Configuration.GetConnectionString("AlvTime_db");
            if (connectionString != null) await MigrationClient.RunMigrations(connectionString, shouldSeed: environment.IsDevelopment());
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
