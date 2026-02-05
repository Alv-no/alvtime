using AlvTime.Business.Options;
using AlvTime.Common.Configuration;
using AlvTime.Persistence.DatabaseModels;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Authorization;
using AlvTimeWebApi.Cors;
using AlvTimeWebApi.ErrorHandling;
using AlvTimeWebApi.Infrastructure;
using AlvTimeWebApi.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using System.Net;

namespace AlvTimeWebApi;

public class Startup
{
    private readonly IHostEnvironment _environment;
    
    public Startup(IHostEnvironment env)
    {
        _environment = env;

        var builder = new ConfigurationBuilder()
            .CommonConfigure<Startup>();

        Configuration = builder.Build();
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAlvtimeServices(Configuration);
        services.AddDbContext<AlvTime_dbContext>(
            options => options.UseSqlServer(Configuration.GetConnectionString("AlvTime")),
            contextLifetime: ServiceLifetime.Scoped, optionsLifetime: ServiceLifetime.Scoped);
        services.AddMvc();
        services.AddAlvtimeAuthentication(Configuration, _environment);
        services.AddMicrosoftGraphClient(Configuration, _environment);
        services.AddScoped<GraphService>();
        services.Configure<TimeEntryOptions>(Configuration.GetSection("TimeEntryOptions"));
        services.AddAlvtimeAuthorization();
        services.AddOpenApi();
        services.AddRazorPages();
        services.AddAlvtimeCorsPolicys(Configuration);
        services.ConfigureLogging(_environment);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
        {
            app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        }

        app.UseErrorHandling(env);

        app.UseRouting();

        if (env.IsDevelopment())
        {
            app.UseCors(CorsExtensions.DevCorsPolicyName);
        }
        else if (env.IsTest())
        {
            app.UseCors(CorsExtensions.TestCorsPolicyName);
            app.UseHttpsRedirection();
        }
        else
        {
            app.UseCors();
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapRazorPages();
            endpoints.MapStaticAssets();
            endpoints.MapOpenApi();
            endpoints.MapScalarApiReference(options =>
            {
                options
                    .WithOAuth2Authentication(oAuth2Options =>
                    {
                        oAuth2Options.ClientId = Configuration["AzureAd:ClientId"];
                        oAuth2Options.Scopes = [Configuration["AzureAd:Domain"]];
                    })
                    .WithTheme(ScalarTheme.Kepler)
                    .WithTitle("AlvTime API")
                    .WithFavicon("/assets/favicon.ico")
                    .WithDarkModeToggle(true);
            });
        });
    }
}