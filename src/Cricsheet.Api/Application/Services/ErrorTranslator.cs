using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;

namespace Cricsheet.Api.Application.Services;

internal sealed class ErrorTranslator : IErrorTranslator
{
    public (int StatusCode, ApiError Error) Translate(Exception exception, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            CosmosException => CreateProviderUnavailable(correlationId),
            _ => CreateProviderUnavailable(correlationId)
        };
    }

    private static (int StatusCode, ApiError Error) CreateProviderUnavailable(string correlationId)
    {
        return (
            StatusCodes.Status503ServiceUnavailable,
            new ApiError(
                "DATA_PROVIDER_UNAVAILABLE",
                "The data provider is currently unavailable. Please try again later.",
                correlationId));
    }
}
