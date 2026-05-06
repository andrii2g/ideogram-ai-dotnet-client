using System.Text.Json;
using System.Text.Json.Serialization;

namespace A2G.Ideogram.Client.Models;

public sealed class IdeogramImageObject
{
    [JsonPropertyName("prompt")]
    public string? Prompt { get; init; }

    [JsonPropertyName("resolution")]
    public string? Resolution { get; init; }

    [JsonPropertyName("is_image_safe")]
    public bool? IsImageSafe { get; init; }

    [JsonPropertyName("seed")]
    public int? Seed { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("style_type")]
    public string? StyleType { get; init; }

    [JsonPropertyName("upscaled_resolution")]
    public string? UpscaledResolution { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
