namespace Cricsheet.Api.Contracts;

internal static class HttpContextCorrelationExtensions
{
    public static string GetCorrelationId(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(CorrelationConstants.ItemKey, out var correlationId) &&
            correlationId is string value &&
            !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return httpContext.TraceIdentifier;
    }
}