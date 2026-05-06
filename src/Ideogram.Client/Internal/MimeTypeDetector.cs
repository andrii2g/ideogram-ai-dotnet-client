namespace A2G.Ideogram.Client.Internal;

internal static class MimeTypeDetector
{
    private static readonly IReadOnlyDictionary<string, string> ExtensionToMimeType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private static readonly IReadOnlySet<string> SupportedContentTypes =
        new HashSet<string>(ExtensionToMimeType.Values, StringComparer.OrdinalIgnoreCase);

    public static string GetContentTypeFromFileName(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !ExtensionToMimeType.TryGetValue(extension, out var contentType))
        {
            throw new ArgumentException($"Unsupported image file extension '{extension}' for '{fileName}'.", nameof(fileName));
        }

        return contentType;
    }

    public static bool IsSupportedContentType(string? contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType) && SupportedContentTypes.Contains(contentType);
    }

    public static string GetDefaultFileExtensionFromContentType(string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        foreach (var pair in ExtensionToMimeType)
        {
            if (string.Equals(pair.Value, contentType, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Key;
            }
        }

        throw new ArgumentException($"Unsupported image MIME type '{contentType}'.", nameof(contentType));
    }

    public static bool IsSupportedFileExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(extension) && ExtensionToMimeType.ContainsKey(extension);
    }
}
