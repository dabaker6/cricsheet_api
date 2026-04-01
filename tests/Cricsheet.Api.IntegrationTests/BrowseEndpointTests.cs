using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Infrastructure.Cosmos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Configuration;

namespace Cricsheet.Api.IntegrationTests;

/// <summary>
/// Integration tests for GET /api/v1/matches/browse.
/// These tests drive out the endpoint behaviour before implementation (TDD).
/// The endpoint is introduced in T015. Until then these tests will fail with 404.
/// </summary>
public sealed class BrowseEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string BaseUrl = "/api/v1/matches/browse";

    private readonly WebApplicationFactory<Program> _factory;

    public BrowseEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // ──────────────────── helpers ────────────────────

    private HttpClient CreateClientWithProvider(IMatchBrowseProvider provider)
    {
        return _factory
            .WithWebHostBuilder(builder =>
                {
                    // Provide valid config so CosmosSettings ValidateOnStart passes.
                    builder.ConfigureAppConfiguration((_, config) =>
                            config.AddInMemoryCollection(new Dictionary<string, string?>
                            {
                                ["Cosmos:AccountEndpoint"] = "https://localhost:8081/",
                                ["Cosmos:DatabaseName"] = "testdb",
                                ["Cosmos:ContainerName"] = "testcontainer"
                            }));

                    builder.ConfigureServices(services =>
                    {
                        // Remove all Cosmos infrastructure so no real connections are attempted.
                        RemoveAllRegistrations<ICosmosClientFactory>(services);
                        // Remove concrete provider registrations so DI validation doesn't fail
                        // when ICosmosClientFactory is absent.
                        RemoveAllRegistrations<IMatchBrowseProvider>(services);
                        RemoveAllRegistrations<IMatchDetailProvider>(services);
                        services.AddSingleton<IMatchBrowseProvider>(provider);
                        services.AddSingleton<IMatchDetailProvider>(new Mock<IMatchDetailProvider>().Object);
                    });
                })
            .CreateClient();
    }

        private static void RemoveAllRegistrations<T>(IServiceCollection services)
        {
            var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
        }

    private static IMatchBrowseProvider StubProvider(BrowseResult result)
    {
        var mock = new Mock<IMatchBrowseProvider>();
        mock.Setup(p => p.BrowseAsync(It.IsAny<BrowseFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return mock.Object;
    }

    private static BrowseResult EmptyResult() => new([], false);

    private static BrowseResult ResultWithItems(int count, bool hasMore = false)
    {
        var items = Enumerable.Range(1, count)
            .Select(i => new MatchSummary(
                $"match{i:D3}",
                ["England", "Australia"],
                "Lords Cricket Ground",
                "Test Championship",
                new DateOnly(2024, 1, i)))
            .ToList();
        return new BrowseResult(items, hasMore);
    }

    // ──────────────────── 200 success ────────────────────

    [Fact]
    public async Task Get_WithGenderFilter_Returns200WithBrowseResponse()
    {
        var client = CreateClientWithProvider(StubProvider(ResultWithItems(3)));

        var response = await client.GetAsync($"{BaseUrl}?gender=male");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").GetArrayLength().Should().Be(3);
        body.GetProperty("hasMore").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Get_WithVenueFilter_Returns200()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync($"{BaseUrl}?venue=Lords");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WithMatchTypeFilter_Returns200()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync($"{BaseUrl}?matchType=Test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WithEventNameFilter_Returns200()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync($"{BaseUrl}?eventName=ICC+World+Cup");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WithTeamFilter_Returns200()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync($"{BaseUrl}?team=England");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WithFromDateFilter_Returns200()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync($"{BaseUrl}?fromDate=2024-01-01");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WithToDateFilter_Returns200()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync($"{BaseUrl}?toDate=2024-12-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WithDateRangeFilter_Returns200()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync($"{BaseUrl}?fromDate=2024-01-01&toDate=2024-12-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WithMultipleFilters_Returns200()
    {
        var client = CreateClientWithProvider(StubProvider(ResultWithItems(2)));

        var response = await client.GetAsync($"{BaseUrl}?gender=male&venue=Lords&matchType=Test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_WhenProviderReturnsEmpty_Returns200WithEmptyItems()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync($"{BaseUrl}?gender=female");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").GetArrayLength().Should().Be(0);
        body.GetProperty("hasMore").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Get_WhenProviderHasMore_Returns200WithHasMoreTrue()
    {
        var client = CreateClientWithProvider(StubProvider(ResultWithItems(10, hasMore: true)));

        var response = await client.GetAsync($"{BaseUrl}?gender=male");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").GetArrayLength().Should().Be(10);
        body.GetProperty("hasMore").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Get_ResponseItems_ContainExpectedMatchSummaryFields()
    {
        var client = CreateClientWithProvider(StubProvider(ResultWithItems(1)));

        var response = await client.GetAsync($"{BaseUrl}?gender=male");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var item = body.GetProperty("items")[0];
        item.TryGetProperty("matchId", out _).Should().BeTrue();
        item.TryGetProperty("teams", out _).Should().BeTrue();
        item.TryGetProperty("venue", out _).Should().BeTrue();
        item.TryGetProperty("competition", out _).Should().BeTrue();
        item.TryGetProperty("date", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Get_CorrelationHeader_EchoedInResponse()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));
        client.DefaultRequestHeaders.Add("X-Correlation-Id", "test-corr-001");

        var response = await client.GetAsync($"{BaseUrl}?gender=male");

        response.Headers.Should().ContainKey("X-Correlation-Id");
        response.Headers.GetValues("X-Correlation-Id").Should().ContainSingle("test-corr-001");
    }

    // ──────────────────── 400 validation failures ────────────────────

    [Fact]
    public async Task Get_NoFilters_Returns400()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_FromDateAfterToDate_Returns400()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync($"{BaseUrl}?fromDate=2024-12-31&toDate=2024-01-01");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_GenderExceedsMaxLength_Returns400()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));
        var longGender = new string('x', 21);

        var response = await client.GetAsync($"{BaseUrl}?gender={longGender}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_400Response_ContainsApiErrorStructure()
    {
        var client = CreateClientWithProvider(StubProvider(EmptyResult()));

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("code", out _).Should().BeTrue();
        body.TryGetProperty("message", out _).Should().BeTrue();
        body.TryGetProperty("correlationId", out _).Should().BeTrue();
    }
}
