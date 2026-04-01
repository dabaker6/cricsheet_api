using Cricsheet.Api.Application.Interfaces;

namespace Cricsheet.Api.Application.Services;

internal interface IBrowseService
{
    Task<BrowseResult> BrowseAsync(BrowseFilter filter, CancellationToken cancellationToken = default);
}

internal sealed class BrowseService : IBrowseService
{
    private readonly IMatchBrowseProvider _matchBrowseProvider;
    private readonly ISummaryMapper _summaryMapper;

    public BrowseService(IMatchBrowseProvider matchBrowseProvider, ISummaryMapper summaryMapper)
    {
        _matchBrowseProvider = matchBrowseProvider;
        _summaryMapper = summaryMapper;
    }

    public async Task<BrowseResult> BrowseAsync(BrowseFilter filter, CancellationToken cancellationToken = default)
    {
        var normalizedFilter = Normalize(filter);
        var providerResult = await _matchBrowseProvider.BrowseAsync(normalizedFilter, cancellationToken).ConfigureAwait(false);
        return _summaryMapper.Map(providerResult);
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
