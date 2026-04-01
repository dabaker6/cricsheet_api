using System.Text.Json.Nodes;

namespace Cricsheet.Api.Application.Interfaces;

internal sealed record MatchDocument(
    string MatchId,
    JsonObject Document);