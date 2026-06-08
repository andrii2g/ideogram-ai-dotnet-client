using System.Text.Json;
using System.Text.Json.Serialization;

namespace A2G.Ideogram.Client.V4.Models;

public sealed class StyleDescription
{
    [JsonPropertyName("aesthetics")]
    public string? Aesthetics { get; init; }

    [JsonPropertyName("lighting")]
    public string? Lighting { get; init; }

    [JsonPropertyName("medium")]
    public string? Medium { get; init; }

    [JsonPropertyName("art_style")]
    public string? ArtStyle { get; init; }

    [JsonPropertyName("photo")]
    public string? Photo { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
