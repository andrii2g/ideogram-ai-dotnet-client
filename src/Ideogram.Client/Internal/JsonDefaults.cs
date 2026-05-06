using System.Text.Json;
using System.Text.Json.Serialization;

namespace A2G.Ideogram.Client.Internal;

internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions Compact = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static readonly JsonSerializerOptions Response = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
