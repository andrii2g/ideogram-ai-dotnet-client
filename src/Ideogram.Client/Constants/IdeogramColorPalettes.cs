namespace Ideogram.Client.Constants;

public static class IdeogramColorPalettes
{
    public const string Ember = "EMBER";
    public const string Fresh = "FRESH";
    public const string Jungle = "JUNGLE";
    public const string Magic = "MAGIC";
    public const string Melon = "MELON";
    public const string Mosaic = "MOSAIC";
    public const string Pastel = "PASTEL";
    public const string Ultramarine = "ULTRAMARINE";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Ember,
        Fresh,
        Jungle,
        Magic,
        Melon,
        Mosaic,
        Pastel,
        Ultramarine
    };
}
