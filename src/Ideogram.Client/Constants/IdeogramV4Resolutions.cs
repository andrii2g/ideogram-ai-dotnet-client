using System.Text.RegularExpressions;

namespace A2G.Ideogram.Client.Constants;

public static partial class IdeogramV4Resolutions
{
    public static bool IsValid(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return ResolutionPattern().IsMatch(value.Trim());
    }

    [GeneratedRegex(@"^\d+x\d+$", RegexOptions.CultureInvariant)]
    private static partial Regex ResolutionPattern();
}
