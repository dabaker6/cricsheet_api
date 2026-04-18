namespace Cricsheet.Api.Application.Interfaces;

internal interface IMatchBrowseProvider
{
    Task<BrowseResult> BrowseAsync(BrowseFilter filter, CancellationToken cancellationToken = default);
}