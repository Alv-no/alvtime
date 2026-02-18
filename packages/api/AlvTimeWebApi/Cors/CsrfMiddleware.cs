using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AlvTimeWebApi.Cors;

public static class CsrfMiddleware
{
    public static void UseCsrfMiddleware(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (!context.Request.Path.StartsWithSegments("/api/auth") && 
                context.Request.Path.StartsWithSegments("/api") && 
                context.Request.Method != HttpMethods.Options && 
                !context.Request.Headers.ContainsKey("X-CSRF"))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Missing X-CSRF header");
                return;
            }
            await next();
        });
    }
}