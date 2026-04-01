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