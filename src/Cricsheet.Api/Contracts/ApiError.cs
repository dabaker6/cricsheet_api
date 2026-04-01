namespace Cricsheet.Api.Contracts;

internal sealed record ApiError(
    string Code,
    string Message,
    string CorrelationId,
    IReadOnlyDictionary<string, string[]>? Details = null);