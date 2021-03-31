using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AlvTimeWebApi.ErrorHandling
{
    static class ErrorHandlingMiddlewareRegistrator
    {
        internal static void UseErrorHandling(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            var options = new ErrorHandlingOptions { ShowStackTrace = false };

            if (!env.IsProduction())
            {
                options.ShowStackTrace = true;
            }

            app.UseMiddleware<ErrorHandlingMiddleware>(options);
        }
    }
}
