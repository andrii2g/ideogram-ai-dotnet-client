using System.Text.Json.Serialization;

namespace Ideogram.Client.Models;

public sealed class ColorPaletteMember
{
    [JsonPropertyName("color_hex")]
    public required string ColorHex { get; init; }

    [JsonPropertyName("color_weight")]
    public double? ColorWeight { get; init; }
}
