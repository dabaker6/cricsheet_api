namespace Cricsheet.Api.Application.Interfaces;

internal sealed record BrowseFilter(
    string? Gender,
    DateOnly? FromDate,
    DateOnly? ToDate,
    string? Venue,
    string? MatchType,
    string? EventName,
    string? Team);