using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Application.Services;
using Cricsheet.Api.Contracts;
using Cricsheet.Api.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Cricsheet.Api.Endpoints;

internal static class DetailEndpoints
{
    public static IEndpointRouteBuilder MapDetailEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/matches");

        group.MapGet("/{matchId}", GetByIdAsync);

        return endpoints;
    }

    private static async Task<IResult> GetByIdAsync(
        string matchId,
        [FromServices] IDetailService detailService,
        [FromServices] IErrorTranslator errorTranslator,
        [FromServices] IValidator<MatchIdRequest> validator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var request = new MatchIdRequest(matchId);
        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            var details = validationResult.Errors
                .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            var validationError = ApiErrors.Create(
                httpContext,
                "VALIDATION_ERROR",
                "The detail request is invalid.",
                details);

            return Results.BadRequest(validationError);
        }

        try
        {
            var detail = await detailService.GetByIdAsync(matchId, cancellationToken).ConfigureAwait(false);
            if (detail is null)
            {
                var notFoundError = ApiErrors.Create(
                    httpContext,
                    "NOT_FOUND",
                    "Match not found.");

                return Results.NotFound(notFoundError);
            }

            return Results.Ok(detail);
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            var (statusCode, error) = errorTranslator.Translate(exception, httpContext.GetCorrelationId());
            return Results.Json(error, statusCode: statusCode);
        }
    }
}
