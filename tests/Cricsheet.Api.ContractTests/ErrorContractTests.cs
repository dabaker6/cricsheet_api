using FluentAssertions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Cricsheet.Api.ContractTests;

public sealed class ErrorContractTests
{
    // ──────────────────── ApiError schema definition ────────────────────

    [Fact]
    public void ApiErrorSchema_ShouldExistInComponents()
    {
        var document = ReadOpenApiDocument();

        document.Components.Schemas.Should().ContainKey("ApiError");
    }

    [Fact]
    public void ApiErrorSchema_ShouldRequireCodeMessageAndCorrelationId()
    {
        var document = ReadOpenApiDocument();
        var schema = document.Components.Schemas["ApiError"];

        schema.Required.Should().Contain("code");
        schema.Required.Should().Contain("message");
        schema.Required.Should().Contain("correlationId");
    }

    [Fact]
    public void ApiErrorSchema_ShouldDefineRequiredPropertiesAsStrings()
    {
        var document = ReadOpenApiDocument();
        var schema = document.Components.Schemas["ApiError"];

        schema.Properties.Should().ContainKey("code");
        schema.Properties.Should().ContainKey("message");
        schema.Properties.Should().ContainKey("correlationId");

        schema.Properties["code"].Type.Should().Be("string");
        schema.Properties["message"].Type.Should().Be("string");
        schema.Properties["correlationId"].Type.Should().Be("string");
    }

    [Fact]
    public void ApiErrorSchema_ShouldDefineOptionalDetailsProperty()
    {
        var document = ReadOpenApiDocument();
        var schema = document.Components.Schemas["ApiError"];

        schema.Properties.Should().ContainKey("details");
        schema.Required.Should().NotContain("details");

        var detailsProperty = schema.Properties["details"];
        detailsProperty.Type.Should().Be("object");
        detailsProperty.AdditionalPropertiesAllowed.Should().BeTrue();
    }

    // ──────────────────── Browse error responses reference ApiError ────────────────────

    [Fact]
    public void BrowseGet400_ShouldReferenceApiErrorSchema()
    {
        var document = ReadOpenApiDocument();
        var operation = document.Paths["/matches/browse"].Operations[OperationType.Get];

        operation.Responses.Should().ContainKey("400");
        var schema = operation.Responses["400"].Content["application/json"].Schema;

        schema.Reference.Should().NotBeNull();
        schema.Reference!.Id.Should().Be("ApiError");
    }

    [Fact]
    public void BrowseGet503_ShouldReferenceApiErrorSchema()
    {
        var document = ReadOpenApiDocument();
        var operation = document.Paths["/matches/browse"].Operations[OperationType.Get];

        operation.Responses.Should().ContainKey("503");
        var schema = operation.Responses["503"].Content["application/json"].Schema;

        schema.Reference.Should().NotBeNull();
        schema.Reference!.Id.Should().Be("ApiError");
    }

    // ──────────────────── Detail error responses reference ApiError ────────────────────

    [Fact]
    public void DetailGet404_ShouldReferenceApiErrorSchema()
    {
        var document = ReadOpenApiDocument();
        var operation = document.Paths["/matches/{matchId}"].Operations[OperationType.Get];

        operation.Responses.Should().ContainKey("404");
        var schema = operation.Responses["404"].Content["application/json"].Schema;

        schema.Reference.Should().NotBeNull();
        schema.Reference!.Id.Should().Be("ApiError");
    }

    [Fact]
    public void DetailGet503_ShouldReferenceApiErrorSchema()
    {
        var document = ReadOpenApiDocument();
        var operation = document.Paths["/matches/{matchId}"].Operations[OperationType.Get];

        operation.Responses.Should().ContainKey("503");
        var schema = operation.Responses["503"].Content["application/json"].Schema;

        schema.Reference.Should().NotBeNull();
        schema.Reference!.Id.Should().Be("ApiError");
    }

    // ──────────────────── helper ────────────────────

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
