namespace A2G.Ideogram.Client.Constants;

public static class IdeogramMagicPrompt
{
    public const string Auto = "AUTO";
    public const string On = "ON";
    public const string Off = "OFF";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Auto,
        On,
        Off
    };
}
