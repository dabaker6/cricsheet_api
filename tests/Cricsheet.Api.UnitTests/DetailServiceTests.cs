using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Infrastructure.Cosmos;
using Cricsheet.Api.Infrastructure.Providers;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Moq;
using Newtonsoft.Json.Linq;

namespace Cricsheet.Api.UnitTests;

public sealed class DetailServiceTests
{
    [Fact]
    public async Task GetByIdAsync_WhenMatchExists_ReturnsFullDocument()
    {
        var matchId = "match-001";
        var row = JObject.Parse(
            """
            {
              "id": "match-001",
              "info": {
                "venue": "Lords",
                "event": { "name": "Test Championship" }
              },
              "innings": [
                { "team": "England" }
              ]
            }
            """);

        var sut = new CosmosMatchDetailProvider(CreateFactoryWithRows([row]).Object);

        var result = await sut.GetByIdAsync(matchId);

        result.Should().NotBeNull();
        result!.MatchId.Should().Be(matchId);
        result.Document["id"]!.GetValue<string>().Should().Be(matchId);
        result.Document["info"]!["venue"]!.GetValue<string>().Should().Be("Lords");
        result.Document["innings"].Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenMatchDoesNotExist_ReturnsNull()
    {
        var sut = new CosmosMatchDetailProvider(CreateFactoryWithRows([]).Object);

        var result = await sut.GetByIdAsync("missing-match");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenDocumentIdMissing_FallsBackToRequestedId()
    {
        var rowWithoutId = JObject.Parse(
            """
            {
              "info": {
                "venue": "The Oval"
              }
            }
            """);

        var sut = new CosmosMatchDetailProvider(CreateFactoryWithRows([rowWithoutId]).Object);

        var result = await sut.GetByIdAsync("fallback-id");

        result.Should().NotBeNull();
        result!.MatchId.Should().Be("fallback-id");
    }

    private static Mock<ICosmosClientFactory> CreateFactoryWithRows(IReadOnlyList<JObject> rows)
    {
        var feedResponseMock = new Mock<FeedResponse<JObject>>();
        feedResponseMock
            .Setup(response => response.GetEnumerator())
            .Returns(() => rows.GetEnumerator());

        var iteratorMock = new Mock<FeedIterator<JObject>>();
        iteratorMock
            .SetupSequence(iterator => iterator.HasMoreResults)
            .Returns(true)
            .Returns(false);
        iteratorMock
            .Setup(iterator => iterator.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedResponseMock.Object);

        var containerMock = new Mock<Container>();
        containerMock
            .Setup(container => container.GetItemQueryIterator<JObject>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string?>(),
                It.IsAny<QueryRequestOptions?>()))
            .Returns(iteratorMock.Object);

        var factoryMock = new Mock<ICosmosClientFactory>();
        factoryMock
            .Setup(factory => factory.GetContainer())
            .Returns(containerMock.Object);

        return factoryMock;
    }
}
