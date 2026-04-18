namespace Cricsheet.Api.Application.Interfaces;

internal interface IMatchDetailProvider
{
    Task<MatchDocument?> GetByIdAsync(string matchId, CancellationToken cancellationToken = default);
}