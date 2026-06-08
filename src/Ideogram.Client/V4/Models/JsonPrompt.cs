using System.Text.Json;
using System.Text.Json.Serialization;

namespace A2G.Ideogram.Client.V4.Models;

public sealed class JsonPrompt
{
    [JsonPropertyName("high_level_description")]
    public string? HighLevelDescription { get; init; }

    [JsonPropertyName("compositional_deconstruction")]
    public CompositionalDeconstruction? CompositionalDeconstruction { get; init; }

    [JsonPropertyName("style_description")]
    public StyleDescription? StyleDescription { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
