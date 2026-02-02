using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System.Linq;

namespace AlvTimeWebApi.Cors
{
    public static class CorsExtensions
    {

        public static string DevCorsPolicyName => "devCorsPolicyName";
        public static string TestCorsPolicyName => "testCorsPolicyName";

        public static void AddAlvtimeCorsPolicys(this IServiceCollection services, IConfiguration configuration)
        {
            string[] AllowedOrigins = configuration["AllowedOrigins"].Split(",");

            string[] AllowedOriginsDev = [ "http://localhost:8080", "http://localhost:8081"];
            
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(AllowedOrigins)
                            .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization)
                            .AllowAnyMethod();
                    }
                );
                
                options.AddPolicy(TestCorsPolicyName, builder =>
                {
                    builder
                        .SetIsOriginAllowed(origin =>
                        {
                            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                            {
                                return AllowedOrigins.Contains(origin) ||
                                        (uri.Scheme == Uri.UriSchemeHttps &&
                                        uri.Host.EndsWith(".westeurope.2.azurestaticapps.net"));
                            }

                            return false;
                        })
                        .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization)
                        .AllowAnyMethod();
                });

                options.AddPolicy(name: DevCorsPolicyName,
                    builder =>
                    {
                        builder.WithOrigins(AllowedOriginsDev).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                    }
                );
            });
        }
    }
}
