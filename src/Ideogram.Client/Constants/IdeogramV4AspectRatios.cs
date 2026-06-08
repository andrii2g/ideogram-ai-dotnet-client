using System.Text.RegularExpressions;

namespace A2G.Ideogram.Client.Constants;

public static partial class IdeogramV4AspectRatios
{
    public const string Auto = "AUTO";
    public const string Ratio1x3 = "1x3";
    public const string Ratio1x2 = "1x2";
    public const string Ratio9x16 = "9x16";
    public const string Ratio10x16 = "10x16";
    public const string Ratio2x3 = "2x3";
    public const string Ratio3x4 = "3x4";
    public const string Ratio4x5 = "4x5";
    public const string Ratio1x1 = "1x1";
    public const string Ratio5x4 = "5x4";
    public const string Ratio4x3 = "4x3";
    public const string Ratio3x2 = "3x2";
    public const string Ratio16x10 = "16x10";
    public const string Ratio16x9 = "16x9";
    public const string Ratio2x1 = "2x1";
    public const string Ratio3x1 = "3x1";

    public static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.Ordinal)
    {
        Auto,
        Ratio1x3,
        Ratio1x2,
        Ratio9x16,
        Ratio10x16,
        Ratio2x3,
        Ratio3x4,
        Ratio4x5,
        Ratio1x1,
        Ratio5x4,
        Ratio4x3,
        Ratio3x2,
        Ratio16x10,
        Ratio16x9,
        Ratio2x1,
        Ratio3x1
    };

    public static string Normalize(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim().ToUpperInvariant().Replace(':', 'x');
    }

    public static bool IsValid(string value)
    {
        var normalized = Normalize(value);
        return Known.Contains(normalized) || RatioPattern().IsMatch(normalized);
    }

    [GeneratedRegex(@"^\d+x\d+$", RegexOptions.CultureInvariant)]
    private static partial Regex RatioPattern();
}
