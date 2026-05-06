namespace A2G.Ideogram.Client;

public sealed class IdeogramFile
{
    public required string FileName { get; init; }
    private static readonly HttpClient SharedHttpClient = new();
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


    public static IdeogramFile FromUrl(
        string url,
        HttpClient? httpClient = null,
        string? fileName = null,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        return FromUrlAsync(url, httpClient, fileName, contentType, cancellationToken).GetAwaiter().GetResult();
    }

    public static async Task<IdeogramFile> FromUrlAsync(
        string url,
        HttpClient? httpClient = null,
        string? fileName = null,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var imageUri) ||
            (imageUri.Scheme != Uri.UriSchemeHttp && imageUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("URL must be an absolute HTTP or HTTPS URL.", nameof(url));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, imageUri);
        using var response = await (httpClient ?? SharedHttpClient)
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        var resolvedContentType = ResolveContentType(response, fileName, contentType);
        var resolvedFileName = ResolveFileName(imageUri, fileName, resolvedContentType);

        return new IdeogramFile
        {
            FileName = resolvedFileName,
            ContentType = resolvedContentType,
            Length = bytes.LongLength,
            OpenReadStream = () => new MemoryStream(bytes, writable: false)
        };
    }

    private static string ResolveContentType(
        HttpResponseMessage response,
        string? fileName,
        string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            if (!Internal.MimeTypeDetector.IsSupportedContentType(contentType))
            {
                throw new ArgumentException($"Unsupported image MIME type '{contentType}'.", nameof(contentType));
            }

            return contentType;
        }

        var headerContentType = response.Content.Headers.ContentType?.MediaType;
        if (Internal.MimeTypeDetector.IsSupportedContentType(headerContentType))
        {
            return headerContentType!;
        }

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            return Internal.MimeTypeDetector.GetContentTypeFromFileName(fileName);
        }

        var urlFileName = Path.GetFileName(response.RequestMessage?.RequestUri?.AbsolutePath);
        if (!string.IsNullOrWhiteSpace(urlFileName) && Internal.MimeTypeDetector.IsSupportedFileExtension(urlFileName))
        {
            return Internal.MimeTypeDetector.GetContentTypeFromFileName(urlFileName);
        }

        throw new InvalidOperationException(
            "Could not determine a supported image MIME type from the response headers or URL. Provide contentType explicitly.");
    }

    private static string ResolveFileName(Uri imageUri, string? fileName, string contentType)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            return fileName;
        }

        var urlFileName = Path.GetFileName(imageUri.AbsolutePath);
        if (!string.IsNullOrWhiteSpace(urlFileName))
        {
            if (Path.HasExtension(urlFileName))
            {
                return urlFileName;
            }

            var extension = Internal.MimeTypeDetector.GetDefaultFileExtensionFromContentType(contentType);
            return $"{urlFileName}{extension}";
        }

        return $"downloaded{Internal.MimeTypeDetector.GetDefaultFileExtensionFromContentType(contentType)}";
    }
}
