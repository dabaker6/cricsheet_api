using Microsoft.Azure.Cosmos;

namespace Cricsheet.Api.Infrastructure.Cosmos;

internal interface ICosmosClientFactory
{
    CosmosClient GetClient();

    Container GetContainer();
}