using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ideogram.Client.Models;

public sealed class ColorPalette
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("members")]
    public IReadOnlyList<ColorPaletteMember>? Members { get; init; }

    public static ColorPalette FromPreset(string name)
    {
        return new ColorPalette
        {
            Name = name
        };
    }

    public static ColorPalette FromMembers(params ColorPaletteMember[] members)
    {
        return new ColorPalette
        {
            Members = members
        };
    }
}
