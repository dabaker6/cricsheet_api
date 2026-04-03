using Cricsheet.Api.Application.Services;
using Cricsheet.Api.Contracts;
using Cricsheet.Api.Validation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;

namespace Cricsheet.Api.UnitTests;

public sealed class ErrorMappingTests
{
    // == ApiError structure ==

    [Fact]
    public void ApiError_WithRequiredProperties_ExposesCodeMessageAndCorrelationId()
    {
        var error = new ApiError("VALIDATION_ERROR", "The request is invalid.", "corr-001");

        error.Code.Should().Be("VALIDATION_ERROR");
        error.Message.Should().Be("The request is invalid.");
        error.CorrelationId.Should().Be("corr-001");
        error.Details.Should().BeNull();
    }

    [Fact]
    public void ApiError_WithDetails_ExposesDetailsDictionary()
    {
        var details = new Dictionary<string, string[]>
        {
            ["team"] = ["Team name is too long."]
        };

        var error = new ApiError("VALIDATION_ERROR", "The request is invalid.", "corr-002", details);

        error.Details.Should().ContainKey("team");
        error.Details!["team"].Should().ContainSingle("Team name is too long.");
    }

    // == ApiErrors factory ==

    [Fact]
    public void ApiErrors_Create_SetsCodeMessageAndCorrelationIdFromContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items[CorrelationConstants.ItemKey] = "corr-factory-001";

        var error = ApiErrors.Create(httpContext, "VALIDATION_ERROR", "bad request");

        error.Code.Should().Be("VALIDATION_ERROR");
        error.Message.Should().Be("bad request");
        error.CorrelationId.Should().Be("corr-factory-001");
        error.Details.Should().BeNull();
    }

    [Fact]
    public void ApiErrors_Create_WithDetails_PassesThroughDetailsDictionary()
    {
        var httpContext = new DefaultHttpContext();
        var details = new Dictionary<string, string[]>
        {
            ["matchId"] = ["matchId must contain only letters, numbers, underscore, or hyphen."]
        };

        var error = ApiErrors.Create(httpContext, "VALIDATION_ERROR", "invalid", details);

        error.Details.Should().ContainKey("matchId");
    }

    // == Validation error details format (used by endpoint handlers) ==

    [Fact]
    public void BrowseValidator_NoFilters_ProducesErrorGroupableByRequestKey()
    {
        var validator = new BrowseFilterRequestValidator();
        var request = new BrowseFilterRequest(null, null, null, null, null, null, null);

        var result = validator.Validate(request);

        var details = result.Errors
            .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "request" : e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        details.Should().ContainKey("request");
        details["request"].Should().ContainSingle(msg => msg.Contains("At least one filter"));
    }

    [Fact]
    public void MatchIdValidator_InvalidChars_ProducesErrorGroupableByMatchIdKey()
    {
        var validator = new MatchIdRequestValidator();
        var request = new MatchIdRequest("invalid id!");

        var result = validator.Validate(request);

        var details = result.Errors
            .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "request" : e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        details.Should().ContainKey("MatchId");
        details["MatchId"].Should().Contain(msg => msg.Contains("only letters, numbers"));
    }

    // == ErrorTranslator - TDD red baseline (tests fail until T024 implements the logic) ==

    [Fact]
    public void ErrorTranslator_ForCosmosException_ReturnsDataProviderUnavailable503()
    {
        var sut = new ErrorTranslator();
        var exception = new CosmosException(
            "Service unavailable",
            System.Net.HttpStatusCode.ServiceUnavailable,
            0,
            "activity-001",
            0);

        var (statusCode, error) = sut.Translate(exception, "corr-cosmos-001");

        statusCode.Should().Be(503);
        error.Code.Should().Be("DATA_PROVIDER_UNAVAILABLE");
        error.CorrelationId.Should().Be("corr-cosmos-001");
    }

    [Fact]
    public void ErrorTranslator_ForGeneralException_ReturnsDataProviderUnavailable503()
    {
        var sut = new ErrorTranslator();
        var exception = new InvalidOperationException("Unexpected provider error");

        var (statusCode, error) = sut.Translate(exception, "corr-general-001");

        statusCode.Should().Be(503);
        error.Code.Should().Be("DATA_PROVIDER_UNAVAILABLE");
        error.CorrelationId.Should().Be("corr-general-001");
    }
}
