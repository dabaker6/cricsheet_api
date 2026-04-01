using FluentAssertions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Cricsheet.Api.ContractTests;

public sealed class BrowseContractTests
{
    [Fact]
    public void BrowsePath_ShouldDefineGetOperationWithExpectedResponseCodes()
    {
        var document = ReadOpenApiDocument();

        document.Paths.Should().ContainKey("/matches/browse");
        var operation = document.Paths["/matches/browse"].Operations[OperationType.Get];

        operation.Responses.Should().ContainKey("200");
        operation.Responses.Should().ContainKey("400");
        operation.Responses.Should().ContainKey("503");
    }

    [Fact]
    public void BrowseGet200_ShouldReferenceBrowseResponseSchema()
    {
        var document = ReadOpenApiDocument();
        var operation = document.Paths["/matches/browse"].Operations[OperationType.Get];

        operation.Responses["200"].Content.Should().ContainKey("application/json");
        var schema = operation.Responses["200"].Content["application/json"].Schema;

        schema.Reference.Should().NotBeNull();
        schema.Reference!.Id.Should().Be("BrowseResponse");
    }

    [Fact]
    public void BrowseResponseSchema_ShouldRequireItemsAndHasMore()
    {
        var document = ReadOpenApiDocument();
        var schema = document.Components.Schemas["BrowseResponse"];

        schema.Required.Should().Contain("items");
        schema.Required.Should().Contain("hasMore");
        schema.Properties.Should().ContainKey("items");
        schema.Properties.Should().ContainKey("hasMore");
        schema.Properties.Should().ContainKey("totalMatched");
    }

    [Fact]
    public void BrowseResponseItems_ShouldCapAtTenAndReferenceMatchSummary()
    {
        var document = ReadOpenApiDocument();
        var schema = document.Components.Schemas["BrowseResponse"];
        var itemsProperty = schema.Properties["items"];

        itemsProperty.Type.Should().Be("array");
        itemsProperty.MaxItems.Should().Be(10);
        itemsProperty.Items.Should().NotBeNull();
        itemsProperty.Items.Reference.Should().NotBeNull();
        itemsProperty.Items.Reference!.Id.Should().Be("MatchSummary");
    }

    [Fact]
    public void MatchSummarySchema_ShouldDefineRequiredFieldsAndConstraints()
    {
        var document = ReadOpenApiDocument();
        var schema = document.Components.Schemas["MatchSummary"];

        schema.Required.Should().Contain(["matchId", "teams", "venue", "competition", "date"]);
        schema.Properties.Should().ContainKey("matchId");
        schema.Properties.Should().ContainKey("teams");
        schema.Properties.Should().ContainKey("venue");
        schema.Properties.Should().ContainKey("competition");
        schema.Properties.Should().ContainKey("date");

        var teamsProperty = schema.Properties["teams"];
        teamsProperty.Type.Should().Be("array");
        teamsProperty.MinItems.Should().Be(2);
        teamsProperty.Items.Should().NotBeNull();
        teamsProperty.Items.Type.Should().Be("string");

        var dateProperty = schema.Properties["date"];
        dateProperty.Type.Should().Be("string");
        dateProperty.Format.Should().Be("date");
    }

    [Fact]
    public void BrowseErrorResponses_ShouldReferenceApiErrorSchema()
    {
        var document = ReadOpenApiDocument();
        var operation = document.Paths["/matches/browse"].Operations[OperationType.Get];

        var badRequestSchema = operation.Responses["400"].Content["application/json"].Schema;
        var serviceUnavailableSchema = operation.Responses["503"].Content["application/json"].Schema;

        badRequestSchema.Reference.Should().NotBeNull();
        badRequestSchema.Reference!.Id.Should().Be("ApiError");
        serviceUnavailableSchema.Reference.Should().NotBeNull();
        serviceUnavailableSchema.Reference!.Id.Should().Be("ApiError");
    }

    private static OpenApiDocument ReadOpenApiDocument()
    {
        var contractPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "..",
                "specs",
                "001-ballbyball-read-api",
                "contracts",
                "openapi.yaml"));

        File.Exists(contractPath).Should().BeTrue($"contract file should exist at {contractPath}");

        using var stream = File.OpenRead(contractPath);
        var reader = new OpenApiStreamReader();
        var document = reader.Read(stream, out var diagnostic);

        diagnostic.Errors.Should().BeEmpty("OpenAPI contract should parse cleanly");
        document.Should().NotBeNull();

        return document;
    }
}
