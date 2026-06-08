using System.Text.Json;
using System.Text.Json.Serialization;

namespace A2G.Ideogram.Client.V4.Models;

public sealed class CompositionalDeconstruction
{
    [JsonPropertyName("background")]
    public string? Background { get; init; }

    [JsonPropertyName("elements")]
    public IReadOnlyList<PromptElement>? Elements { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
