using System.Text.Json.Nodes;
using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Infrastructure.Cosmos;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace Cricsheet.Api.Infrastructure.Providers;

internal sealed class CosmosMatchDetailProvider : IMatchDetailProvider
{
    private readonly ICosmosClientFactory _cosmosClientFactory;

    public CosmosMatchDetailProvider(ICosmosClientFactory cosmosClientFactory)
    {
        _cosmosClientFactory = cosmosClientFactory;
    }

    public async Task<MatchDocument?> GetByIdAsync(string matchId, CancellationToken cancellationToken = default)
    {
        var queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.id = @matchId")
            .WithParameter("@matchId", matchId);

        var container = _cosmosClientFactory.GetContainer();
        var iterator = container.GetItemQueryIterator<JObject>(queryDefinition);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            var row = response.FirstOrDefault();
            if (row is null)
            {
                continue;
            }

            var id = row["id"]?.Value<string>() ?? matchId;
            var documentNode = JsonNode.Parse(row.ToString()) as JsonObject;
            if (documentNode is null)
            {
                return null;
            }

            return new MatchDocument(id, documentNode);
        }

        return null;
    }
}