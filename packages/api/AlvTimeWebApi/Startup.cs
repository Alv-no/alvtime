using AlvTime.Business.Events;
using AlvTime.Business.Options;
using AlvTime.Business.TimeRegistration.Events;
using AlvTime.Common.Configuration;
using AlvTime.Persistence.DatabaseModels;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Authorization;
using AlvTimeWebApi.BackgroundServices;
using AlvTimeWebApi.Cors;
using AlvTimeWebApi.ErrorHandling;
using AlvTimeWebApi.Infrastructure;
using AlvTimeWebApi.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using System;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.SqlServer;

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
        services.Configure<OutpostOptions>(Configuration.GetSection("Outpost"));
        services.AddAlvtimeAuthorization();
        services.AddOpenApi(o => 
        {
            o.AddDocumentTransformer<OpenApiLoginTransformer>();
            o.AddOperationTransformer<DefaultHeaderTransformer>();
        });

        var connectionString = Configuration.GetConnectionString("AlvTime")
            ?? throw new InvalidOperationException("Connection string 'AlvTime' not found.");

        // Masstransit has gone commercial, goes for Wolverine instead
        services.AddWolverine(opts =>
        {
            opts.Policies.DisableConventionalLocalRouting();

            opts.PersistMessagesWithSqlServer(
                connectionString,
                "wolverine");

            opts.UseEntityFrameworkCoreTransactions();
            opts.Policies.AutoApplyTransactions();

            opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
            // opts.Policies.UseDurableInboxOnAllListeners(); // Dont need inbox pattern for now

            // Explicitly register a handler (like AddConsumer<T>)
            opts.Discovery.IncludeType<WebhookOutboxProcessor>();

            // Conventional local routing is disabled, so route webhook events to a durable local queue explicitly
            opts.PublishMessage<WebhookEvent<TimeEntryCreatedEvent>>().ToLocalQueue("webhooks");
            opts.PublishMessage<WebhookEvent<TimeEntryUpdatedEvent>>().ToLocalQueue("webhooks");
            opts.LocalQueue("webhooks").UseDurableInbox();

            opts.OnException<System.Net.Http.HttpRequestException>()
                .RetryWithCooldown(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30))
                .Then.ScheduleRetry(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(30), TimeSpan.FromHours(2))
                .Then.MoveToErrorQueue();
        });

        // Without a secret webhooks are skipped, which is only allowed locally
        var clientSecret = Configuration["Outpost:ApiKey"];
        if (string.IsNullOrEmpty(clientSecret) && !_environment.IsDevelopment())
        {
            throw new InvalidOperationException("Outpost:ApiKey is not configured.");
        }

        services.AddHttpClient("WebhookOutpost", c =>
        {
            c.BaseAddress = new Uri(Configuration["Outpost:BaseUrl"]!);
            c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientSecret);

            c.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddRazorPages();
        services.AddAlvtimeCorsPolicys(Configuration);
        services.ConfigureLogging(_environment);
        services.AddMemoryCache();
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

        app.UseCsrfMiddleware();
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
                    .WithTheme(ScalarTheme.Kepler)
                    .WithTitle("AlvTime API")
                    .WithFavicon("/assets/favicon.ico");
            });
        });
    }
}