using Cricsheet.Api.Contracts;

namespace Cricsheet.Api.Application.Interfaces;

internal interface IErrorTranslator
{
    (int StatusCode, ApiError Error) Translate(Exception exception, string correlationId);
}
