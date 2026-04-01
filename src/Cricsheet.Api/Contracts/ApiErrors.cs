namespace Cricsheet.Api.Contracts;

internal static class ApiErrors
{
    public static ApiError Create(
        HttpContext httpContext,
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? details = null)
    {
        return new ApiError(
            code,
            message,
            httpContext.GetCorrelationId(),
            details);
    }
}