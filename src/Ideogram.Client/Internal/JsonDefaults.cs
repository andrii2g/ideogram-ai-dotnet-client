using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ideogram.Client.Internal;

internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions Compact = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
