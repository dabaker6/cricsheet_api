using Azure.Identity;
using Cricsheet.Api.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Cricsheet.Api.Infrastructure.Cosmos;

internal sealed class ManagedIdentityCosmosClientFactory : ICosmosClientFactory, IDisposable
{
    private readonly CosmosSettings _settings;
    private readonly Lazy<CosmosClient> _cosmosClient;

    public ManagedIdentityCosmosClientFactory(IOptions<CosmosSettings> settings)
    {
        _settings = settings.Value;
        _cosmosClient = new Lazy<CosmosClient>(CreateClientInternal, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public CosmosClient GetClient()
    {
        return _cosmosClient.Value;
    }

    public Container GetContainer()
    {
        return GetClient().GetContainer(_settings.DatabaseName, _settings.ContainerName);
    }

    public void Dispose()
    {
        if (_cosmosClient.IsValueCreated)
        {
            _cosmosClient.Value.Dispose();
        }
    }

    private CosmosClient CreateClientInternal()
    {
        var tokenCredential = BuildTokenCredential(_settings.ManagedIdentityClientId);
        var cosmosClientOptions = new CosmosClientOptions();

        return new CosmosClient(_settings.AccountEndpoint, tokenCredential, cosmosClientOptions);
    }

    private static DefaultAzureCredential BuildTokenCredential(string? managedIdentityClientId)
    {
        if (string.IsNullOrWhiteSpace(managedIdentityClientId))
        {
            return new DefaultAzureCredential();
        }

        return new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = managedIdentityClientId
            });
    }
}