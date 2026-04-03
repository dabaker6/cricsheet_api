using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Infrastructure.Cosmos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Cricsheet.Api.IntegrationTests;

/// <summary>
/// Integration tests for structured error contracts across 400, 404, and 503 responses.
/// US3: Frontend-Usable Errors.
/// 503 tests are TDD red until T025 wires exception handling via IErrorTranslator.
/// </summary>
public sealed class ErrorResponseTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string BrowseUrl = "/api/v1/matches/browse";
    private const string DetailUrl = "/api/v1/matches";

    private readonly WebApplicationFactory<Program> _factory;

    public ErrorResponseTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // ──────────────────── 400 VALIDATION_ERROR contract ────────────────────

    [Fact]
    public async Task Browse_400Response_HasValidationErrorCode()
    {
        var client = CreateClient(StubBrowseProvider(new BrowseResult([], false)));

        var response = await client.GetAsync(BrowseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetString().Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task Browse_400Response_HasMessageCorrelationIdAndDetails()
    {
        var client = CreateClient(StubBrowseProvider(new BrowseResult([], false)));

        var response = await client.GetAsync(BrowseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("message", out _).Should().BeTrue();
        body.TryGetProperty("correlationId", out _).Should().BeTrue();
        body.TryGetProperty("details", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Detail_400Response_HasValidationErrorCodeAndDetails()
    {
        var client = CreateClient(StubBrowseProvider(new BrowseResult([], false)));

        var response = await client.GetAsync($"{DetailUrl}/bad%20id");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetString().Should().Be("VALIDATION_ERROR");
        body.TryGetProperty("details", out _).Should().BeTrue();
    }

    // ──────────────────── 404 NOT_FOUND contract ────────────────────

    [Fact]
    public async Task Detail_404Response_HasNotFoundCode()
    {
        var client = CreateClient(
            StubBrowseProvider(new BrowseResult([], false)),
            StubDetailProvider(null));

        var response = await client.GetAsync($"{DetailUrl}/missing-match-id");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetString().Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Detail_404Response_HasMessageAndCorrelationId()
    {
        var client = CreateClient(
            StubBrowseProvider(new BrowseResult([], false)),
            StubDetailProvider(null));

        var response = await client.GetAsync($"{DetailUrl}/missing-match-id");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("message", out _).Should().BeTrue();
        body.TryGetProperty("correlationId", out _).Should().BeTrue();
    }

    // ──────────────────── 503 DATA_PROVIDER_UNAVAILABLE contract (TDD red — wired in T025) ────────────────────

    [Fact]
    public async Task Browse_WhenProviderThrowsCosmosException_Returns503()
    {
        var throwingProvider = StubThrowingBrowseProvider(
            new CosmosException(
                "Service unavailable",
                System.Net.HttpStatusCode.ServiceUnavailable,
                0,
                "activity-browse-503",
                0));

        var client = CreateClient(throwingProvider);

        var response = await client.GetAsync($"{BrowseUrl}?gender=male");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Browse_WhenProviderThrowsCosmosException_Returns503WithDataProviderUnavailableCode()
    {
        var throwingProvider = StubThrowingBrowseProvider(
            new CosmosException(
                "Service unavailable",
                System.Net.HttpStatusCode.ServiceUnavailable,
                0,
                "activity-browse-code",
                0));

        var client = CreateClient(throwingProvider);

        var response = await client.GetAsync($"{BrowseUrl}?gender=male");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetString().Should().Be("DATA_PROVIDER_UNAVAILABLE");
        body.TryGetProperty("correlationId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Detail_WhenProviderThrowsCosmosException_Returns503()
    {
        var throwingDetailProvider = StubThrowingDetailProvider(
            new CosmosException(
                "Service unavailable",
                System.Net.HttpStatusCode.ServiceUnavailable,
                0,
                "activity-detail-503",
                0));

        var client = CreateClient(
            StubBrowseProvider(new BrowseResult([], false)),
            throwingDetailProvider);

        var response = await client.GetAsync($"{DetailUrl}/match-001");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Detail_WhenProviderThrowsCosmosException_Returns503WithDataProviderUnavailableCode()
    {
        var throwingDetailProvider = StubThrowingDetailProvider(
            new CosmosException(
                "Service unavailable",
                System.Net.HttpStatusCode.ServiceUnavailable,
                0,
                "activity-detail-code",
                0));

        var client = CreateClient(
            StubBrowseProvider(new BrowseResult([], false)),
            throwingDetailProvider);

        var response = await client.GetAsync($"{DetailUrl}/match-001");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("code").GetString().Should().Be("DATA_PROVIDER_UNAVAILABLE");
        body.TryGetProperty("correlationId", out _).Should().BeTrue();
    }

    // ──────────────────── helpers ────────────────────

    private HttpClient CreateClient(
        IMatchBrowseProvider browseProvider,
        IMatchDetailProvider? detailProvider = null)
    {
        detailProvider ??= new Mock<IMatchDetailProvider>().Object;

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

                    services.AddSingleton(browseProvider);
                    services.AddSingleton(detailProvider);
                });
            })
            .CreateClient();
    }

    private static IMatchBrowseProvider StubBrowseProvider(BrowseResult result)
    {
        var mock = new Mock<IMatchBrowseProvider>();
        mock.Setup(p => p.BrowseAsync(It.IsAny<BrowseFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return mock.Object;
    }

    private static IMatchBrowseProvider StubThrowingBrowseProvider(Exception exception)
    {
        var mock = new Mock<IMatchBrowseProvider>();
        mock.Setup(p => p.BrowseAsync(It.IsAny<BrowseFilter>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        return mock.Object;
    }

    private static IMatchDetailProvider StubDetailProvider(MatchDocument? document)
    {
        var mock = new Mock<IMatchDetailProvider>();
        mock.Setup(p => p.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        return mock.Object;
    }

    private static IMatchDetailProvider StubThrowingDetailProvider(Exception exception)
    {
        var mock = new Mock<IMatchDetailProvider>();
        mock.Setup(p => p.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
        return mock.Object;
    }

    private static void RemoveAllRegistrations<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
