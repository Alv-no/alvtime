using System.Collections.Generic;
using System.Linq;
using AlvTime.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlvTimeWebApi.ErrorHandling;

internal static class ErrorCollectionExtensions
{
    internal static ValidationProblemDetails ToValidationProblemDetails(this IEnumerable<Error> errors, string title)
    {
        var errorDict = errors
            .GroupBy(e => e.ErrorCode)
            .ToDictionary(errorCodeGrouping => errorCodeGrouping.Key.ToString(), errorCodeGrouping => errorCodeGrouping.Select(e => e.Description).ToArray());
        return new ValidationProblemDetails(errorDict)
        {
            Title = title,
            Status = StatusCodes.Status400BadRequest,
            Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5"
        };
    }
}