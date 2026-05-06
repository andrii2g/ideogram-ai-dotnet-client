namespace Ideogram.Client.Constants;

public static class IdeogramUpscaleFactors
{
    public const string X1 = "X1";
    public const string X2 = "X2";
    public const string X4 = "X4";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        X1,
        X2,
        X4
    };
}
