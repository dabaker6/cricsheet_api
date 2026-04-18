namespace Cricsheet.Api.Validation;

internal sealed record BrowseFilterRequest(
    string? Gender,
    DateOnly? FromDate,
    DateOnly? ToDate,
    string? Venue,
    string? MatchType,
    string? EventName,
    string? Team);