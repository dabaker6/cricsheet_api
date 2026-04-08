using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Cricsheet.Api.Configuration;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by ASP.NET Core options binding via reflection.")]
internal sealed class CosmosSettings
{
    public const string SectionName = "Cosmos";

    [Required]
    [Url]
    public string AccountEndpoint { get; init; } = string.Empty;

    public string? AccountKey { get; init; }

    [Required]
    public string DatabaseName { get; init; } = string.Empty;

    [Required]
    public string ContainerName { get; init; } = string.Empty;

    public string? ManagedIdentityClientId { get; init; }
}