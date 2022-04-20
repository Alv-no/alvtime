using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace AlvTimeWebApi.Authorization;

public static class SwaggerExtensions
{
    public static void AddSwaggerExtensions(this IServiceCollection services, IConfiguration configuration)
    {
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
                                    $"https://login.microsoftonline.com/{configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                            TokenUrl = new Uri(
                                $"https://login.microsoftonline.com/{configuration["AzureAd:TenantId"]}/oauth2/v2.0/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                {configuration["AzureAd:Domain"], "Access the API"}
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
                        new[] {configuration["AzureAd:Domain"]}
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
                                 {configuration["AzureAd:Domain"], "Access the API"}
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
                        new[] {configuration["AzureAd:Domain"]}
                    }
                });
            });
    }
}