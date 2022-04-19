using System;
using System.Collections.Generic;
using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
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
using Microsoft.OpenApi.Models;

namespace AlvTimeWebApi
{
    public class Startup
    {
        public Startup(IHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAlvtimeServices(Configuration);
            services.AddDbContext<AlvTime_dbContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("AlvTime_db")),
                contextLifetime: ServiceLifetime.Scoped, optionsLifetime: ServiceLifetime.Scoped);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddAlvtimeAuthentication(Configuration);
            services.Configure<TimeEntryOptions>(Configuration.GetSection("TimeEntryOptions"));
            services.AddAlvtimeAuthorization();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Alvtime API", Version = "v1"});

                // AAD SSO
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Authenticate using Azure AD with your account.",
                    Name = "Azure AD",
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl =
                                new Uri(
                                    $"https://login.microsoftonline.com/{Configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                            TokenUrl = new Uri(
                                $"https://login.microsoftonline.com/{Configuration["AzureAd:TenantId"]}/oauth2/v2.0/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                {Configuration["AzureAd:Domain"], "Access the API"}
                            }
                        }
                    }
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "oauth2"}
                        },
                        new[] {Configuration["AzureAd:Domain"]}
                    }
                });

                // Bearer token (for use with debugging and Ahre-Ketil Lillehagen)
                c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Name = "Bearer token",
                    Description = "Authenticate using a raw bearer token. This is helpful when you want to use Ahre-Ketil for testing. You can paste his PAT here.",
                    Scheme = "Bearer",
                    BearerFormat = "Raw JWT or PAT",
                    Type = SecuritySchemeType.Http,
                    Flows = new OpenApiOAuthFlows
                    {
                         ClientCredentials = new OpenApiOAuthFlow
                         {
                             Scopes = new Dictionary<string, string>
                             {
                                 {Configuration["AzureAd:Domain"], "Access the API"}
                             }
                         }
                    },
                    In = ParameterLocation.Header,
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "bearer"}
                        },
                        new[] {Configuration["AzureAd:Domain"]}
                    }
                });
            });
            services.AddRazorPages();
            services.AddAlvtimeCorsPolicys(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
}