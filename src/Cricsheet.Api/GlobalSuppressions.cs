using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Foundational application contracts are defined before later tasks introduce their consumers.",
    Scope = "type",
    Target = "~T:Cricsheet.Api.Application.Interfaces.BrowseFilter")]
[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Foundational application contracts are defined before later tasks introduce their consumers.",
    Scope = "type",
    Target = "~T:Cricsheet.Api.Application.Interfaces.BrowseResult")]
[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Foundational application contracts are defined before later tasks introduce their consumers.",
    Scope = "type",
    Target = "~T:Cricsheet.Api.Application.Interfaces.MatchDocument")]
[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Foundational application contracts are defined before later tasks introduce their consumers.",
    Scope = "type",
    Target = "~T:Cricsheet.Api.Application.Interfaces.MatchSummary")]
[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Validation request models are consumed by endpoint binding in subsequent tasks.",
    Scope = "type",
    Target = "~T:Cricsheet.Api.Validation.BrowseFilterRequest")]
[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Validation request models are consumed by endpoint binding in subsequent tasks.",
    Scope = "type",
    Target = "~T:Cricsheet.Api.Validation.MatchIdRequest")]
[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Validators are resolved through FluentValidation service scanning and consumed by endpoint handlers in subsequent tasks.",
    Scope = "type",
    Target = "~T:Cricsheet.Api.Validation.BrowseFilterRequestValidator")]
[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Validators are resolved through FluentValidation service scanning and consumed by endpoint handlers in subsequent tasks.",
    Scope = "type",
    Target = "~T:Cricsheet.Api.Validation.MatchIdRequestValidator")]
[assembly: SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Factory is instantiated by dependency injection registration in Program.cs.",
    Scope = "type",
    Target = "~T:Cricsheet.Api.Infrastructure.Cosmos.ManagedIdentityCosmosClientFactory")]