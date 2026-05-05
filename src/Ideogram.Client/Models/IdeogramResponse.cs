using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ideogram.Client.Models;

public sealed class IdeogramResponse
{
    [JsonPropertyName("created")]
    public string? Created { get; init; }

    [JsonPropertyName("data")]
    public IReadOnlyList<IdeogramImageObject> Data { get; init; } = [];

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
