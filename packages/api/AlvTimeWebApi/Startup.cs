using System.Reflection;
using AlvTime.Business.Options;
using AlvTime.Persistence.DatabaseModels;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Authorization;
using AlvTimeWebApi.Cors;
using AlvTimeWebApi.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AlvTimeWebApi;

public class Startup
{
    public Startup(IHostEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

        Configuration = builder.Build();
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAlvtimeServices(Configuration);
        services.AddDbContext<AlvTime_dbContext>(
            options => options.UseSqlServer(Configuration.GetConnectionString("AlvTime_db")),
            contextLifetime: ServiceLifetime.Scoped, optionsLifetime: ServiceLifetime.Scoped);
        services.AddMvc();
        services.AddAlvtimeAuthentication(Configuration);
        services.Configure<TimeEntryOptions>(Configuration.GetSection("TimeEntryOptions"));
        services.AddAlvtimeAuthorization();
        services.AddSwaggerExtensions(Configuration);
        services.AddRazorPages();
        services.AddAlvtimeCorsPolicys(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
        {
            app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        }

        app.UseStaticFiles();
        app.UseErrorHandling(env);

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alvtime API v1");
            c.OAuthClientId(Configuration["AzureAd:ClientId"]);
            c.OAuthUsePkce();
            c.OAuthScopeSeparator(" ");
        });

        app.UseRouting();

        if (env.IsDevelopment())
        {
            app.UseCors(CorsExtensions.DevCorsPolicyName);
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
        });
    }
}