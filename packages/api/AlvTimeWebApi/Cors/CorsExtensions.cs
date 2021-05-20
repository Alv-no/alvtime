using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System;

namespace AlvTimeWebApi.Cors
{
    public static class CorsExtensions
    {

        public static string DevCorsPolicyName => "devCorsPolicyName";

        public static void AddAlvtimeCorsPolicys(this IServiceCollection services, IConfiguration configuration)
        {
            string[] AllowedOrigins = configuration["AllowedOrigins"].Split(",");

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

                options.AddPolicy(name: DevCorsPolicyName,
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    }
                );
            });
        }
    }
}
