using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Application.Services;
using Cricsheet.Api.Contracts;
using Cricsheet.Api.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cricsheet.Api.Endpoints;

internal static class BrowseEndpoints
{
    public static IEndpointRouteBuilder MapBrowseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/matches");

        group.MapGet("/browse", BrowseAsync);

        return endpoints;
    }

    private static async Task<IResult> BrowseAsync(
        string? gender,
        DateOnly? fromDate,
        DateOnly? toDate,
        string? venue,
        string? matchType,
        string? eventName,
        string? team,
        [FromServices] IBrowseService browseService,
        [FromServices] IErrorTranslator errorTranslator,
        [FromServices] IValidator<BrowseFilterRequest> validator,
        [FromServices] ILoggerFactory loggerFactory,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var request = new BrowseFilterRequest(gender, fromDate, toDate, venue, matchType, eventName, team);
        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validationResult.IsValid)
        {
            var details = validationResult.Errors
                .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            var apiError = ApiErrors.Create(
                httpContext,
                "VALIDATION_ERROR",
                "The browse request is invalid.",
                details);

            return Results.BadRequest(apiError);
        }

        var filter = new BrowseFilter(gender, fromDate, toDate, venue, matchType, eventName, team);
        try
        {
            var browseResult = await browseService.BrowseAsync(filter, cancellationToken).ConfigureAwait(false);
            return Results.Ok(browseResult);
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            var logger = loggerFactory.CreateLogger("BrowseEndpoints");
            logger.LogError(
                exception,
                "Browse request failed. CorrelationId: {CorrelationId}",
                httpContext.GetCorrelationId());

            var (statusCode, error) = errorTranslator.Translate(exception, httpContext.GetCorrelationId());
            return Results.Json(error, statusCode: statusCode);
        }
    }
}
