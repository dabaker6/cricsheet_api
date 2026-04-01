using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Infrastructure.Cosmos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Cricsheet.Api.IntegrationTests;

public sealed class DetailEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string BaseUrl = "/api/v1/matches";

    private readonly WebApplicationFactory<Program> _factory;

    public DetailEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_WithExistingMatchId_Returns200WithDetailPayload()
    {
        var document = new JsonObject
        {
            ["id"] = "match-001",
            ["info"] = new JsonObject
            {
                ["venue"] = "Lords"
            }
        };

        var detailProvider = StubDetailProvider(new MatchDocument("match-001", document));
        var client = CreateClient(detailProvider);

        var response = await client.GetAsync($"{BaseUrl}/match-001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("matchId").GetString().Should().Be("match-001");
        body.TryGetProperty("document", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Get_WithUnknownMatchId_Returns404WithApiErrorEnvelope()
    {
        var client = CreateClient(StubDetailProvider(null));

        var response = await client.GetAsync($"{BaseUrl}/missing-id");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("code", out _).Should().BeTrue();
        body.TryGetProperty("message", out _).Should().BeTrue();
        body.TryGetProperty("correlationId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Get_WithInvalidMatchIdCharacters_Returns400()
    {
        var client = CreateClient(StubDetailProvider(null));

        var response = await client.GetAsync($"{BaseUrl}/bad%20id");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_CorrelationHeader_EchoedInResponse()
    {
        var client = CreateClient(StubDetailProvider(null));
        client.DefaultRequestHeaders.Add("X-Correlation-Id", "detail-corr-001");

        var response = await client.GetAsync($"{BaseUrl}/match-001");

        response.Headers.Should().ContainKey("X-Correlation-Id");
        response.Headers.GetValues("X-Correlation-Id").Should().ContainSingle("detail-corr-001");
    }

    private HttpClient CreateClient(IMatchDetailProvider detailProvider)
    {
        return _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Cosmos:AccountEndpoint"] = "https://localhost:8081/",
                        ["Cosmos:DatabaseName"] = "testdb",
                        ["Cosmos:ContainerName"] = "testcontainer"
                    }));

                builder.ConfigureServices(services =>
                {
                    RemoveAllRegistrations<ICosmosClientFactory>(services);
                    RemoveAllRegistrations<IMatchBrowseProvider>(services);
                    RemoveAllRegistrations<IMatchDetailProvider>(services);

                    services.AddSingleton<IMatchBrowseProvider>(StubBrowseProvider());
                    services.AddSingleton(detailProvider);
                });
            })
            .CreateClient();
    }

    private static IMatchBrowseProvider StubBrowseProvider()
    {
        var mock = new Mock<IMatchBrowseProvider>();
        mock
            .Setup(provider => provider.BrowseAsync(It.IsAny<BrowseFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BrowseResult([], false));
        return mock.Object;
    }

    private static IMatchDetailProvider StubDetailProvider(MatchDocument? document)
    {
        var mock = new Mock<IMatchDetailProvider>();
        mock
            .Setup(provider => provider.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        return mock.Object;
    }

    private static void RemoveAllRegistrations<T>(IServiceCollection services)
    {
        var descriptors = services.Where(descriptor => descriptor.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
