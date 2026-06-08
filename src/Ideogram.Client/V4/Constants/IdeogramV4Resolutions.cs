namespace A2G.Ideogram.Client.Constants.V4;

public static class Resolutions
{
    public static bool IsValid(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return All.Contains(value.Trim());
    }

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        "2048x2048",
        "1440x2880",
        "2880x1440",
        "1664x2496",
        "2496x1664",
        "1792x2240",
        "2240x1792",
        "1440x2560",
        "2560x1440",
        "1600x2560",
        "2560x1600",
        "1728x2304",
        "2304x1728",
        "1296x3168",
        "3168x1296",
        "1152x2944",
        "2944x1152",
        "1248x3328",
        "3328x1248",
        "1280x3072",
        "3072x1280",
        "1024x3072",
        "3072x1024"
    };
}
