using System.Text.Json.Serialization;

namespace A2G.Ideogram.Client.V4.Models;

public sealed class DescribeResponse
{
    [JsonPropertyName("json_prompt")]
    public JsonPrompt? JsonPrompt { get; init; }
}
