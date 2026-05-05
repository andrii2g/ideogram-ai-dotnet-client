using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ideogram.Client.Models;

public sealed class IdeogramErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("error")]
    public JsonElement? Error { get; init; }

    [JsonPropertyName("detail")]
    public JsonElement? Detail { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
