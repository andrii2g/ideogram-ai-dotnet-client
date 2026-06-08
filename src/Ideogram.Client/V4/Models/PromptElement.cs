using System.Text.Json;
using System.Text.Json.Serialization;

namespace A2G.Ideogram.Client.V4.Models;

public sealed class PromptElement
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("bbox")]
    public IReadOnlyList<int>? BoundingBox { get; init; }

    [JsonPropertyName("desc")]
    public string? Description { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
