using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Contracts;

namespace Cricsheet.Api.Application.Services;

internal sealed class ErrorTranslator : IErrorTranslator
{
    public (int StatusCode, ApiError Error) Translate(Exception exception, string correlationId)
    {
        throw new NotImplementedException("ErrorTranslator is not yet implemented; will be completed in T024.");
    }
}
