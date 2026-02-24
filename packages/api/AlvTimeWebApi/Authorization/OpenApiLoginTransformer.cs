using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Threading;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Authorization;

public class OpenApiLoginTransformer(IHostEnvironment environment) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (environment.IsDevelopment())
        {
            document.Info.Description = "<a href=\"http://localhost:8081/api/auth/login?returnUrl=http://localhost:8081/scalar/v1\">Trykk her for å autorisere</a>";
        }
        else if (environment.IsTest())
        {
            document.Info.Description = "<a href=\"https://api.test-alvtime.no/api/auth/login?returnUrl=https://api.test-alvtime.no/scalar/v1\">Trykk her for å autorisere</a>";
        }
        else if (environment.IsProduction())
        {
            document.Info.Description = "<a href=\"https://api.alvtime.no/api/auth/login?returnUrl=https://api.alvtime.no/scalar/v1\">Trykk her for å autorisere</a>";
        }

        return Task.CompletedTask;
    }
}
