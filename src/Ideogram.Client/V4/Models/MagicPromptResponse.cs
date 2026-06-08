using System.Text.Json.Serialization;

namespace A2G.Ideogram.Client.V4.Models;

public sealed class MagicPromptResponse
{
    [JsonPropertyName("json_prompt")]
    public JsonPrompt? JsonPrompt { get; init; }

    [JsonPropertyName("aspect_ratio")]
    public string? AspectRatio { get; init; }
}
