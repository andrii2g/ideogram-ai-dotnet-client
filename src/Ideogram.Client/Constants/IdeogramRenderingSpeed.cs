namespace Ideogram.Client.Constants;

public static class IdeogramRenderingSpeed
{
    public const string Flash = "FLASH";
    public const string Turbo = "TURBO";
    public const string Default = "DEFAULT";
    public const string Quality = "QUALITY";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Flash,
        Turbo,
        Default,
        Quality
    };

    public static readonly IReadOnlySet<string> TransparentAllowed = new HashSet<string>(StringComparer.Ordinal)
    {
        Turbo,
        Default,
        Quality
    };
}
