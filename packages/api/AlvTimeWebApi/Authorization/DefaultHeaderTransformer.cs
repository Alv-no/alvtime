using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Threading;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Authorization;

public class DefaultHeaderTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-CSRF",
            In = ParameterLocation.Header,
            Description = "Custom header will enforce preflight and mitigate som XSS-attacks",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            },
            Example = new Microsoft.OpenApi.Any.OpenApiString("1")
        });

        return Task.CompletedTask;
    }
}
