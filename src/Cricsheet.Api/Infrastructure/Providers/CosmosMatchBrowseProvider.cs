using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Infrastructure.Cosmos;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Cricsheet.Api.Infrastructure.Providers;

internal sealed class CosmosMatchBrowseProvider : IMatchBrowseProvider
{
    private const int BrowseLimit = 10;
    private readonly ICosmosClientFactory _cosmosClientFactory;

    public CosmosMatchBrowseProvider(ICosmosClientFactory cosmosClientFactory)
    {
        _cosmosClientFactory = cosmosClientFactory;
    }

    public async Task<BrowseResult> BrowseAsync(BrowseFilter filter, CancellationToken cancellationToken = default)
    {
        var queryDefinition = BuildQuery(filter);
        var container = _cosmosClientFactory.GetContainer();
        var iterator = container.GetItemQueryIterator<JObject>(queryDefinition);

        var summaries = new List<MatchSummary>();

        while (iterator.HasMoreResults && summaries.Count < BrowseLimit + 1)
        {
            var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            foreach (var row in response)
            {
                var summary = TryMapSummary(row);
                if (summary is not null)
                {
                    summaries.Add(summary);
                    if (summaries.Count >= BrowseLimit + 1)
                    {
                        break;
                    }
                }
            }
        }

        var hasMore = summaries.Count > BrowseLimit;
        var items = hasMore ? summaries.Take(BrowseLimit).ToList() : summaries;

        return new BrowseResult(items, hasMore);
    }

    private static QueryDefinition BuildQuery(BrowseFilter filter)
    {
        var clauses = new List<string>();
        var query = new QueryDefinition(
            "SELECT TOP 11 c.id, c.info.teams, c.info.venue, c.info.event.name, c.info.dates FROM c");

        if (!string.IsNullOrWhiteSpace(filter.Gender))
        {
            clauses.Add("c.info.gender = @gender");
            query = query.WithParameter("@gender", filter.Gender);
        }

        if (!string.IsNullOrWhiteSpace(filter.Venue))
        {
            clauses.Add("c.info.venue = @venue");
            query = query.WithParameter("@venue", filter.Venue);
        }

        if (!string.IsNullOrWhiteSpace(filter.MatchType))
        {
            clauses.Add("c.info.match_type = @matchType");
            query = query.WithParameter("@matchType", filter.MatchType);
        }

        if (!string.IsNullOrWhiteSpace(filter.EventName))
        {
            clauses.Add("c.info.event.name = @eventName");
            query = query.WithParameter("@eventName", filter.EventName);
        }

        if (!string.IsNullOrWhiteSpace(filter.Team))
        {
            clauses.Add("ARRAY_CONTAINS(c.info.teams, @team)");
            query = query.WithParameter("@team", filter.Team);
        }

        if (filter.FromDate is not null)
        {
            clauses.Add("c.info.dates[0] >= @fromDate");
            query = query.WithParameter("@fromDate", filter.FromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        if (filter.ToDate is not null)
        {
            clauses.Add("c.info.dates[0] <= @toDate");
            query = query.WithParameter("@toDate", filter.ToDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        if (clauses.Count > 0)
        {
            query = new QueryDefinition(
                "SELECT TOP 11 c.id, c.info.teams, c.info.venue, c.info.event.name, c.info.dates FROM c WHERE " +
                string.Join(" AND ", clauses));

            if (!string.IsNullOrWhiteSpace(filter.Gender))
            {
                query = query.WithParameter("@gender", filter.Gender);
            }

            if (!string.IsNullOrWhiteSpace(filter.Venue))
            {
                query = query.WithParameter("@venue", filter.Venue);
            }

            if (!string.IsNullOrWhiteSpace(filter.MatchType))
            {
                query = query.WithParameter("@matchType", filter.MatchType);
            }

            if (!string.IsNullOrWhiteSpace(filter.EventName))
            {
                query = query.WithParameter("@eventName", filter.EventName);
            }

            if (!string.IsNullOrWhiteSpace(filter.Team))
            {
                query = query.WithParameter("@team", filter.Team);
            }

            if (filter.FromDate is not null)
            {
                query = query.WithParameter("@fromDate", filter.FromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }

            if (filter.ToDate is not null)
            {
                query = query.WithParameter("@toDate", filter.ToDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
        }

        return query;
    }

    private static MatchSummary? TryMapSummary(JObject row)
    {
        var matchId = row["id"]?.Value<string>();
        var venue = row.SelectToken("venue")?.Value<string>();
        var competition = row.SelectToken("name")?.Value<string>();
        var dateText = row.SelectToken("dates[0]")?.Value<string>();
        var teams = row.SelectToken("teams")?.Values<string?>()
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();

        if (string.IsNullOrWhiteSpace(matchId) ||
            string.IsNullOrWhiteSpace(venue) ||
            string.IsNullOrWhiteSpace(competition) ||
            string.IsNullOrWhiteSpace(dateText) ||
            teams is null ||
            teams.Count < 2 ||
            !DateOnly.TryParse(dateText, out var matchDate))
        {
            return null;
        }

        return new MatchSummary(matchId, teams, venue, competition, matchDate);
    }
}