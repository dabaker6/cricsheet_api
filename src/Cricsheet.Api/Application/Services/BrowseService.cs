using Cricsheet.Api.Application.Interfaces;

namespace Cricsheet.Api.Application.Services;

internal interface IBrowseService
{
    Task<BrowseResult> BrowseAsync(BrowseFilter filter, CancellationToken cancellationToken = default);
}

internal sealed class BrowseService : IBrowseService
{
    private readonly IMatchBrowseProvider _matchBrowseProvider;

    public BrowseService(IMatchBrowseProvider matchBrowseProvider)
    {
        _matchBrowseProvider = matchBrowseProvider;
    }

    public Task<BrowseResult> BrowseAsync(BrowseFilter filter, CancellationToken cancellationToken = default)
    {
        var normalizedFilter = Normalize(filter);
        return _matchBrowseProvider.BrowseAsync(normalizedFilter, cancellationToken);
    }

    private static BrowseFilter Normalize(BrowseFilter filter)
    {
        return new BrowseFilter(
            NormalizeString(filter.Gender),
            filter.FromDate,
            filter.ToDate,
            NormalizeString(filter.Venue),
            NormalizeString(filter.MatchType),
            NormalizeString(filter.EventName),
            NormalizeString(filter.Team));
    }

    private static string? NormalizeString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
