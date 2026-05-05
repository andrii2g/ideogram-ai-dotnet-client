using System;
using System.IO;

namespace Ideogram.Client;

public sealed class IdeogramFile
{
    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required Func<Stream> OpenReadStream { get; init; }

    public long? Length { get; init; }

    public static IdeogramFile FromPath(string path, string? contentType = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File '{fullPath}' was not found.", fullPath);
        }

        var fileName = Path.GetFileName(fullPath);
        var resolvedContentType = contentType ?? Internal.MimeTypeDetector.GetContentTypeFromFileName(fileName);
        var fileInfo = new FileInfo(fullPath);

        return new IdeogramFile
        {
            FileName = fileName,
            ContentType = resolvedContentType,
            Length = fileInfo.Length,
            OpenReadStream = () => File.OpenRead(fullPath)
        };
    }
}
