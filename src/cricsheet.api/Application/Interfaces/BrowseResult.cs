namespace Cricsheet.Api.Application.Interfaces;

internal sealed record BrowseResult(
    IReadOnlyList<MatchSummary> Items,
    bool HasMore,
    int? TotalMatched = null);