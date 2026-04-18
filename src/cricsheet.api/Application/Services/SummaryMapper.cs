using Cricsheet.Api.Application.Interfaces;

namespace Cricsheet.Api.Application.Services;

internal interface ISummaryMapper
{
    BrowseResult Map(BrowseResult source);
}

internal sealed class SummaryMapper : ISummaryMapper
{
    public BrowseResult Map(BrowseResult source)
    {
        var mappedItems = source.Items
            .Select(MapSummary)
            .ToList();

        return new BrowseResult(mappedItems, source.HasMore, source.TotalMatched);
    }

    private static MatchSummary MapSummary(MatchSummary source)
    {
        var teams = source.Teams
            .Where(team => !string.IsNullOrWhiteSpace(team))
            .Select(team => team.Trim())
            .ToList();

        return new MatchSummary(
            source.MatchId.Trim(),
            teams,
            source.Venue.Trim(),
            source.Competition.Trim(),
            source.Date);
    }
}
