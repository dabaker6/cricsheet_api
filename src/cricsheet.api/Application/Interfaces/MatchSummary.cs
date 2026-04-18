namespace Cricsheet.Api.Application.Interfaces;

internal sealed record MatchSummary(
    string MatchId,
    IReadOnlyList<string> Teams,
    string Venue,
    string Competition,
    DateOnly Date);