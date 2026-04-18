using Cricsheet.Api.Application.Interfaces;

namespace Cricsheet.Api.Application.Services;

internal interface IDetailService
{
    Task<MatchDocument?> GetByIdAsync(string matchId, CancellationToken cancellationToken = default);
}

internal sealed class DetailService : IDetailService
{
    private readonly IMatchDetailProvider _matchDetailProvider;

    public DetailService(IMatchDetailProvider matchDetailProvider)
    {
        _matchDetailProvider = matchDetailProvider;
    }

    public Task<MatchDocument?> GetByIdAsync(string matchId, CancellationToken cancellationToken = default)
    {
        var normalizedMatchId = NormalizeMatchId(matchId);
        return _matchDetailProvider.GetByIdAsync(normalizedMatchId, cancellationToken);
    }

    private static string NormalizeMatchId(string matchId)
    {
        return matchId.Trim();
    }
}
