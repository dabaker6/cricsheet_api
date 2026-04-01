using FluentAssertions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Cricsheet.Api.ContractTests;

public sealed class DetailContractTests
{
    [Fact]
    public void DetailPath_ShouldDefineGetOperationWithExpectedResponseCodes()
    {
        var document = ReadOpenApiDocument();

        document.Paths.Should().ContainKey("/matches/{matchId}");
        var operation = document.Paths["/matches/{matchId}"].Operations[OperationType.Get];

        operation.Responses.Should().ContainKey("200");
        operation.Responses.Should().ContainKey("404");
        operation.Responses.Should().ContainKey("503");
    }

    [Fact]
    public void DetailPath_ShouldDefineRequiredMatchIdPathParameter()
    {
        var document = ReadOpenApiDocument();
        var operation = document.Paths["/matches/{matchId}"].Operations[OperationType.Get];

        operation.Parameters.Should().ContainSingle(parameter =>
            parameter.In == ParameterLocation.Path &&
            parameter.Name == "matchId" &&
            parameter.Required);
    }

    [Fact]
    public void DetailGet200_ShouldReferenceDetailResponseSchema()
    {
        var document = ReadOpenApiDocument();
        var operation = document.Paths["/matches/{matchId}"].Operations[OperationType.Get];

        operation.Responses["200"].Content.Should().ContainKey("application/json");
        var schema = operation.Responses["200"].Content["application/json"].Schema;

        schema.Reference.Should().NotBeNull();
        schema.Reference!.Id.Should().Be("DetailResponse");
    }

    [Fact]
    public void DetailResponseSchema_ShouldRequireMatchIdAndDocument()
    {
        var document = ReadOpenApiDocument();
        var schema = document.Components.Schemas["DetailResponse"];

        schema.Required.Should().Contain("matchId");
        schema.Required.Should().Contain("document");
        schema.Properties.Should().ContainKey("matchId");
        schema.Properties.Should().ContainKey("document");

        var matchIdProperty = schema.Properties["matchId"];
        matchIdProperty.Type.Should().Be("string");

        var documentProperty = schema.Properties["document"];
        documentProperty.Type.Should().Be("object");
        documentProperty.AdditionalPropertiesAllowed.Should().BeTrue();
    }

    [Fact]
    public void DetailErrorResponses_ShouldReferenceApiErrorSchema()
    {
        var document = ReadOpenApiDocument();
        var operation = document.Paths["/matches/{matchId}"].Operations[OperationType.Get];

        var notFoundSchema = operation.Responses["404"].Content["application/json"].Schema;
        var serviceUnavailableSchema = operation.Responses["503"].Content["application/json"].Schema;

        notFoundSchema.Reference.Should().NotBeNull();
        notFoundSchema.Reference!.Id.Should().Be("ApiError");
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
