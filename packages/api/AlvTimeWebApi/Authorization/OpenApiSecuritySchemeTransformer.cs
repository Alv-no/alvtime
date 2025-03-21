using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

namespace AlvTimeWebApi.Authorization;

public class OpenApiSecuritySchemeTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document, 
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var securitySchema =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri($"https://login.microsoftonline.com/{configuration["AzureAd:TenantId"]}/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { configuration["AzureAd:Domain"], "Access the API" }
                        }
                    }
                }
            };

        var securityRequirement =
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme,
                        },
                    },
                    []
                }
            };

        document.SecurityRequirements.Add(securityRequirement);
        document.Components = new OpenApiComponents()
        {
            SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>()
            {
                { "Bearer", securitySchema }
            }
        };

        return Task.CompletedTask;
    }
}