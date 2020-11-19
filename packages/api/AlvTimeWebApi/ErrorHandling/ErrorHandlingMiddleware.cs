using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlvTimeWebApi.ErrorHandling
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ErrorHandlingOptions _options;

        public ErrorHandlingMiddleware(RequestDelegate next, ErrorHandlingOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var problem = new ProblemDetails
                {
                    Status = context.Response.StatusCode,
                    Title = e.Message,
                    Extensions =
                    {
                        { "traceId", context.TraceIdentifier }
                    },
                    Detail = _options.ShowStackTrace ? e.ToString() : string.Empty,
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }
    }

    public class ErrorHandlingOptions
    {
        public bool ShowStackTrace { get; set; }
    }
}
