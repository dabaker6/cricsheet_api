using Cricsheet.Api.Application.Interfaces;
using Cricsheet.Api.Infrastructure.Cosmos;
using Cricsheet.Api.Infrastructure.Providers;
using Cricsheet.Api.Validation;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Moq;
using Newtonsoft.Json.Linq;

namespace Cricsheet.Api.UnitTests;

public sealed class BrowseFilterRequestValidatorTests
{
    private readonly BrowseFilterRequestValidator _sut = new();

    [Fact]
    public void Validate_NoFiltersSet_FailsWithAtLeastOneFilterError()
    {
        var request = new BrowseFilterRequest(null, null, null, null, null, null, null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("At least one filter"));
    }

    [Fact]
    public void Validate_GenderOnly_IsValid()
    {
        var request = new BrowseFilterRequest("male", null, null, null, null, null, null);
        _sut.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_VenueOnly_IsValid()
    {
        var request = new BrowseFilterRequest(null, null, null, "Lords", null, null, null);
        _sut.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MatchTypeOnly_IsValid()
    {
        var request = new BrowseFilterRequest(null, null, null, null, "Test", null, null);
        _sut.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EventNameOnly_IsValid()
    {
        var request = new BrowseFilterRequest(null, null, null, null, null, "ICC World Cup", null);
        _sut.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TeamOnly_IsValid()
    {
        var request = new BrowseFilterRequest(null, null, null, null, null, null, "England");
        _sut.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FromDateOnly_IsValid()
    {
        var request = new BrowseFilterRequest(null, new DateOnly(2024, 1, 1), null, null, null, null, null);
        _sut.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ToDateOnly_IsValid()
    {
        var request = new BrowseFilterRequest(null, null, new DateOnly(2024, 12, 31), null, null, null, null);
        _sut.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FromDateBeforeToDate_IsValid()
    {
        var request = new BrowseFilterRequest(null, new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), null, null, null, null);
        _sut.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FromDateEqualToToDate_IsValid()
    {
        var date = new DateOnly(2024, 6, 15);
        var request = new BrowseFilterRequest(null, date, date, null, null, null, null);
        _sut.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FromDateAfterToDate_FailsWithDateRangeError()
    {
        var request = new BrowseFilterRequest(null, new DateOnly(2024, 12, 31), new DateOnly(2024, 1, 1), null, null, null, null);

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorMessage.Contains("fromDate"));
    }

    [Fact]
    public void Validate_GenderAtMaxLength_IsValid()
    {
        var request = new BrowseFilterRequest(new string('x', 20), null, null, null, null, null, null);
        _sut.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_GenderExceedsMaxLength_FailsValidation()
    {
        var request = new BrowseFilterRequest(new string('x', 21), null, null, null, null, null, null);
        _sut.Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_VenueExceedsMaxLength_FailsValidation()
    {
        var request = new BrowseFilterRequest(null, null, null, new string('x', 201), null, null, null);
        _sut.Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_MatchTypeExceedsMaxLength_FailsValidation()
    {
        var request = new BrowseFilterRequest(null, null, null, null, new string('x', 51), null, null);
        _sut.Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EventNameExceedsMaxLength_FailsValidation()
    {
        var request = new BrowseFilterRequest(null, null, null, null, null, new string('x', 201), null);
        _sut.Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TeamExceedsMaxLength_FailsValidation()
    {
        var request = new BrowseFilterRequest(null, null, null, null, null, null, new string('x', 151));
        _sut.Validate(request).IsValid.Should().BeFalse();
    }
}

public sealed class CosmosMatchBrowseProviderCappingTests
{
    private static readonly string[] MatchDates = ["2024-01-15"];
    private static readonly string[] MatchTeams = ["England", "Australia"];

    private static JObject CreateMatchRow(string matchId) => JObject.FromObject(new
    {
        id = matchId,
        venue = "Lords Cricket Ground",
        name = "Test Championship",
        dates = MatchDates,
        teams = MatchTeams
    });

    private static List<JObject> CreateMatchRows(int count) =>
        Enumerable.Range(1, count)
            .Select(i => CreateMatchRow($"match{i:D3}"))
            .ToList();

    private static Mock<ICosmosClientFactory> SetupFactoryMock(List<JObject> rows)
    {
        var mockResponse = new Mock<FeedResponse<JObject>>();
        mockResponse
            .Setup(r => r.GetEnumerator())
            .Returns(() => rows.GetEnumerator());

        var iteratorMock = new Mock<FeedIterator<JObject>>();
        iteratorMock
            .SetupSequence(i => i.HasMoreResults)
            .Returns(true)
            .Returns(false);
        iteratorMock
            .Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        var containerMock = new Mock<Container>();
        containerMock
            .Setup(c => c.GetItemQueryIterator<JObject>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string?>(),
                It.IsAny<QueryRequestOptions?>()))
            .Returns(iteratorMock.Object);

        var factoryMock = new Mock<ICosmosClientFactory>();
        factoryMock.Setup(f => f.GetContainer()).Returns(containerMock.Object);

        return factoryMock;
    }

    [Fact]
    public async Task BrowseAsync_ElevenResults_ReturnsTenItemsAndHasMoreTrue()
    {
        var sut = new CosmosMatchBrowseProvider(SetupFactoryMock(CreateMatchRows(11)).Object);
        var filter = new BrowseFilter("male", null, null, null, null, null, null);

        var result = await sut.BrowseAsync(filter);

        result.Items.Should().HaveCount(10);
        result.HasMore.Should().BeTrue();
    }

    [Fact]
    public async Task BrowseAsync_TenResults_ReturnsTenItemsAndHasMoreFalse()
    {
        var sut = new CosmosMatchBrowseProvider(SetupFactoryMock(CreateMatchRows(10)).Object);
        var filter = new BrowseFilter("male", null, null, null, null, null, null);

        var result = await sut.BrowseAsync(filter);

        result.Items.Should().HaveCount(10);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task BrowseAsync_FiveResults_ReturnsFiveItemsAndHasMoreFalse()
    {
        var sut = new CosmosMatchBrowseProvider(SetupFactoryMock(CreateMatchRows(5)).Object);
        var filter = new BrowseFilter("male", null, null, null, null, null, null);

        var result = await sut.BrowseAsync(filter);

        result.Items.Should().HaveCount(5);
        result.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task BrowseAsync_ZeroResults_ReturnsEmptyAndHasMoreFalse()
    {
        var sut = new CosmosMatchBrowseProvider(SetupFactoryMock(CreateMatchRows(0)).Object);
        var filter = new BrowseFilter("male", null, null, null, null, null, null);

        var result = await sut.BrowseAsync(filter);

        result.Items.Should().BeEmpty();
        result.HasMore.Should().BeFalse();
    }
}
