using A2G.Ideogram.Client.Internal;
using A2G.Ideogram.Client.Models;
using A2G.Ideogram.Client.Constants.V4;
using A2G.Ideogram.Client.V4.Models;
using A2G.Ideogram.Client.V4.Validation;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using V4RemixRequest = A2G.Ideogram.Client.V4.Models.RemixRequest;

namespace A2G.Ideogram.Client;

public sealed class IdeogramV4Client : IIdeogramV4Client, IDisposable
{
    private static readonly string[] ForbiddenDownloadDefaultHeaderNames =
    [
        "Api-Key",
        "Authorization",
        "X-Api-Key",
        "X-API-Key",
        "X-Ideogram-Api-Key"
    ];

    private static readonly string[] RequestIdHeaderNames =
    [
        "x-request-id",
        "request-id",
        "x-amzn-requestid"
    ];

    private readonly IdeogramClientOptions _options;
    private readonly string _apiKey;
    private readonly HttpClient _apiHttpClient;
    private readonly HttpClient _downloadHttpClient;
    private readonly bool _disposeApiHttpClient;
    private readonly bool _disposeDownloadHttpClient;
    private bool _disposed;

    public IdeogramV4Client(IdeogramClientOptions options)
        : this(CreateOwnedHttpClient(), CreateOwnedHttpClient(), options, true, true)
    {
    }

    public IdeogramV4Client(HttpClient apiHttpClient, IdeogramClientOptions options, bool disposeApiHttpClient = false)
        : this(apiHttpClient, CreateOwnedHttpClient(), options, disposeApiHttpClient, true)
    {
    }

    public IdeogramV4Client(
        HttpClient apiHttpClient,
        HttpClient downloadHttpClient,
        IdeogramClientOptions options,
        bool disposeApiHttpClient = false,
        bool disposeDownloadHttpClient = false)
    {
        ArgumentNullException.ThrowIfNull(apiHttpClient);
        ArgumentNullException.ThrowIfNull(downloadHttpClient);
        ArgumentNullException.ThrowIfNull(options);

        ValidateOptions(options);

        if (ReferenceEquals(apiHttpClient, downloadHttpClient))
        {
            throw new ArgumentException("API and download HttpClient instances must be different.", nameof(downloadHttpClient));
        }

        if (apiHttpClient.DefaultRequestHeaders.Contains("Api-Key"))
        {
            throw new InvalidOperationException("Do not configure Api-Key in HttpClient.DefaultRequestHeaders; Ideogram API authentication is added per request only.");
        }

        if (HasForbiddenDownloadDefaultHeader(downloadHttpClient))
        {
            throw new InvalidOperationException("Download HttpClient must be headerless for credentials; remove forbidden credential headers from DefaultRequestHeaders.");
        }

        _options = options;
        _apiKey = options.ApiKey;
        _apiHttpClient = apiHttpClient;
        _downloadHttpClient = downloadHttpClient;
        _disposeApiHttpClient = disposeApiHttpClient;
        _disposeDownloadHttpClient = disposeDownloadHttpClient;

        EnsureUserAgent(_apiHttpClient, options.UserAgent);
    }

    public async Task<IdeogramResponse> GenerateFromTextAsync(
        GenerateFromTextRequest request,
        CancellationToken cancellationToken = default)
    {
        RequestValidator.Validate(request);

        using var content = new MultipartFormDataContent();
        HttpContentBuilder.AddRequiredString(content, "text_prompt", request.TextPrompt);
        HttpContentBuilder.AddOptionalString(content, "resolution", request.Resolution);
        HttpContentBuilder.AddOptionalString(content, "rendering_speed", request.RenderingSpeed);
        HttpContentBuilder.AddOptionalBool(content, "enable_copyright_detection", request.EnableCopyrightDetection);

        return await SendMultipartAsync("/v1/ideogram-v4/generate", content, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IdeogramResponse> GenerateFromJsonAsync(
        GenerateFromJsonRequest request,
        CancellationToken cancellationToken = default)
    {
        RequestValidator.Validate(request);

        using var content = new MultipartFormDataContent();
        HttpContentBuilder.AddJson(content, "json_prompt", request.JsonPrompt);
        HttpContentBuilder.AddOptionalString(content, "resolution", request.Resolution);
        HttpContentBuilder.AddOptionalString(content, "rendering_speed", request.RenderingSpeed);
        HttpContentBuilder.AddOptionalBool(content, "enable_copyright_detection", request.EnableCopyrightDetection);

        return await SendMultipartAsync("/v1/ideogram-v4/generate", content, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IdeogramResponse> RemixAsync(
        V4RemixRequest request,
        CancellationToken cancellationToken = default)
    {
        RequestValidator.Validate(request);

        using var content = new MultipartFormDataContent();
        HttpContentBuilder.AddFile(content, "image", request.Image);
        HttpContentBuilder.AddRequiredString(content, "text_prompt", request.TextPrompt);
        HttpContentBuilder.AddOptionalInt(content, "image_weight", request.ImageWeight);
        HttpContentBuilder.AddOptionalString(content, "resolution", request.Resolution);
        HttpContentBuilder.AddOptionalString(content, "rendering_speed", request.RenderingSpeed);
        HttpContentBuilder.AddOptionalBool(content, "enable_copyright_detection", request.EnableCopyrightDetection);

        return await SendMultipartAsync("/v1/ideogram-v4/remix", content, cancellationToken).ConfigureAwait(false);
    }

    public Task<MagicPromptResponse> GenerateMagicPromptAsync(
        MagicPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        RequestValidator.Validate(request);

        var payload = new
        {
            text_prompt = request.TextPrompt,
            aspect_ratio = string.IsNullOrWhiteSpace(request.AspectRatio) ? null : AspectRatiosV4.Normalize(request.AspectRatio)
        };

        return SendJsonAsync<object, MagicPromptResponse>("/v1/ideogram-v4/magic-prompt", payload, cancellationToken);
    }

    public async Task<DescribeResponse> DescribeAsync(
        DescribeRequest request,
        CancellationToken cancellationToken = default)
    {
        RequestValidator.Validate(request);

        using var content = new MultipartFormDataContent();
        HttpContentBuilder.AddFile(content, "image_file", request.ImageFile);
        HttpContentBuilder.AddOptionalBool(content, "include_bbox", request.IncludeBoundingBoxes);

        return await SendMultipartAsync<DescribeResponse>("/v1/ideogram-v4/describe", content, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> DownloadImagesAsync(
        IdeogramResponse response,
        string outputDirectory,
        string fileNamePrefix = "ideogram",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileNamePrefix);

        Directory.CreateDirectory(outputDirectory);

        var savedPaths = new List<string>();
        for (var index = 0; index < response.Data.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var image = response.Data[index];
            if (string.IsNullOrWhiteSpace(image.Url))
            {
                continue;
            }

            var extensionToken = DetermineExtensionToken(image.Url);
            var safePrefix = SanitizeFileNameSegment(fileNamePrefix);
            var seedToken = image.Seed?.ToString(CultureInfo.InvariantCulture) ?? "na";
            var baseFileName = $"{safePrefix}_{index:00}_seed-{seedToken}";
            var outputPath = await FindAvailableGeneratedPathAsync(
                outputDirectory,
                baseFileName,
                extensionToken,
                cancellationToken).ConfigureAwait(false);

            var savedPath = await DownloadImageAsync(image.Url, outputPath, cancellationToken).ConfigureAwait(false);
            savedPaths.Add(savedPath);
        }

        return savedPaths;
    }

    public async Task<string> DownloadImageAsync(
        string imageUrl,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var imageUri) ||
            (imageUri.Scheme != Uri.UriSchemeHttp && imageUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("imageUrl must be an absolute HTTP or HTTPS URL.", nameof(imageUrl));
        }

        var fullOutputPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(fullOutputPath))
        {
            throw new IOException($"The output file '{fullOutputPath}' already exists.");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.DownloadTimeout);
        var operationToken = timeoutCts.Token;

        try
        {
            if (HasForbiddenDownloadDefaultHeader(_downloadHttpClient))
            {
                throw new InvalidOperationException("Download HttpClient must be headerless for credentials; remove forbidden credential headers from DefaultRequestHeaders.");
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, imageUri);
            using var response = await _downloadHttpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            await using var input = await response.Content.ReadAsStreamAsync(operationToken).ConfigureAwait(false);
            await using var output = new FileStream(
                fullOutputPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await input.CopyToAsync(output, operationToken).ConfigureAwait(false);
            await output.FlushAsync(operationToken).ConfigureAwait(false);

            return fullOutputPath;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException($"Ideogram image download exceeded {_options.DownloadTimeout}: {imageUri}", ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_disposeApiHttpClient)
        {
            _apiHttpClient.Dispose();
        }

        if (_disposeDownloadHttpClient)
        {
            _downloadHttpClient.Dispose();
        }

        _disposed = true;
    }

    private Task<IdeogramResponse> SendMultipartAsync(
        string relativePath,
        MultipartFormDataContent content,
        CancellationToken cancellationToken)
    {
        return SendMultipartAsync<IdeogramResponse>(relativePath, content, cancellationToken);
    }

    private async Task<TResponse> SendMultipartAsync<TResponse>(
        string relativePath,
        MultipartFormDataContent content,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        ArgumentNullException.ThrowIfNull(content);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.RequestTimeout);
        var operationToken = timeoutCts.Token;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, BuildApiUri(relativePath));
            AddApiKeyHeader(request);
            request.Content = content;

            using var response = await _apiHttpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationToken)
                .ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync(operationToken).ConfigureAwait(false);
            return DeserializeResponse<TResponse>(response, body, relativePath);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException($"Ideogram API request to '{relativePath}' exceeded {_options.RequestTimeout}.", ex);
        }
    }

    private async Task<TResponse> SendJsonAsync<TRequest, TResponse>(
        string relativePath,
        TRequest payload,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        ArgumentNullException.ThrowIfNull(payload);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_options.RequestTimeout);
        var operationToken = timeoutCts.Token;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, BuildApiUri(relativePath));
            AddApiKeyHeader(request);
            var json = JsonSerializer.Serialize(payload, JsonDefaults.Compact);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _apiHttpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationToken)
                .ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync(operationToken).ConfigureAwait(false);
            return DeserializeResponse<TResponse>(response, body, relativePath);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException($"Ideogram API request to '{relativePath}' exceeded {_options.RequestTimeout}.", ex);
        }
    }

    private TResponse DeserializeResponse<TResponse>(
        HttpResponseMessage response,
        string body,
        string relativePath)
    {
        var requestId = TryGetRequestId(response.Headers);
        if (!response.IsSuccessStatusCode)
        {
            var message = BuildErrorMessage(response.StatusCode, body);
            throw new IdeogramApiException(response.StatusCode, message, body, relativePath, requestId);
        }

        try
        {
            var model = JsonSerializer.Deserialize<TResponse>(body, JsonDefaults.Response);
            if (model is null)
            {
                throw new InvalidOperationException($"Response body for '{relativePath}' deserialized to null.");
            }

            return model;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Could not deserialize Ideogram response for '{relativePath}'. Raw body: {body}",
                ex);
        }
    }

    private void AddApiKeyHeader(HttpRequestMessage request)
    {
        if (_apiHttpClient.DefaultRequestHeaders.Contains("Api-Key"))
        {
            throw new InvalidOperationException("Do not configure Api-Key in HttpClient.DefaultRequestHeaders; Ideogram API authentication is added per request only.");
        }

        if (!request.Headers.TryAddWithoutValidation("Api-Key", _apiKey))
        {
            throw new InvalidOperationException("Could not add the Ideogram Api-Key request header.");
        }
    }

    private static HttpClient CreateOwnedHttpClient()
    {
        return new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    private static void EnsureUserAgent(HttpClient client, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return;
        }

        if (!client.DefaultRequestHeaders.UserAgent.Any())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }
    }

    private static void ValidateOptions(IdeogramClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new ArgumentException("IdeogramClientOptions.ApiKey is required.", nameof(options));
        }

        if (!options.BaseUri.IsAbsoluteUri || options.BaseUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("IdeogramClientOptions.BaseUri must be an absolute HTTPS URI.", nameof(options));
        }

        ValidateTimeout(options.RequestTimeout, nameof(options.RequestTimeout));
        ValidateTimeout(options.DownloadTimeout, nameof(options.DownloadTimeout));
    }

    private static void ValidateTimeout(TimeSpan timeout, string propertyName)
    {
        if (timeout <= TimeSpan.Zero || timeout == Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(propertyName, $"{propertyName} must be a positive, finite value.");
        }
    }

    private static bool HasForbiddenDownloadDefaultHeader(HttpClient client)
    {
        if (client.DefaultRequestHeaders.Authorization is not null)
        {
            return true;
        }

        foreach (var name in ForbiddenDownloadDefaultHeaderNames)
        {
            if (client.DefaultRequestHeaders.Contains(name))
            {
                return true;
            }
        }

        return false;
    }

    private Uri BuildApiUri(string relativePath)
    {
        return new Uri(_options.BaseUri, relativePath);
    }

    private static string BuildErrorMessage(HttpStatusCode statusCode, string body)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                var error = JsonSerializer.Deserialize<IdeogramErrorResponse>(body, JsonDefaults.Response);
                if (error is not null)
                {
                    if (!string.IsNullOrWhiteSpace(error.Message))
                    {
                        return $"Ideogram API request failed with {(int)statusCode} ({statusCode}): {error.Message}";
                    }

                    var detailText = GetJsonElementText(error.Detail);
                    if (!string.IsNullOrWhiteSpace(detailText))
                    {
                        return $"Ideogram API request failed with {(int)statusCode} ({statusCode}): {detailText}";
                    }

                    var errorText = GetJsonElementText(error.Error);
                    if (!string.IsNullOrWhiteSpace(errorText))
                    {
                        return $"Ideogram API request failed with {(int)statusCode} ({statusCode}): {errorText}";
                    }
                }
            }
            catch (JsonException)
            {
            }
        }

        if (!string.IsNullOrWhiteSpace(body))
        {
            return $"Ideogram API request failed with {(int)statusCode} ({statusCode}): {body}";
        }

        return $"Ideogram API request failed with {(int)statusCode} ({statusCode}).";
    }

    private static string? GetJsonElementText(JsonElement? element)
    {
        if (element is null)
        {
            return null;
        }

        if (element.Value.ValueKind == JsonValueKind.String)
        {
            return element.Value.GetString();
        }

        return element.Value.GetRawText();
    }

    private static string? TryGetRequestId(HttpResponseHeaders headers)
    {
        foreach (var headerName in RequestIdHeaderNames)
        {
            if (headers.TryGetValues(headerName, out var values))
            {
                return values.FirstOrDefault();
            }
        }

        return null;
    }

    private static string DetermineExtensionToken(string imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            return "png";
        }

        var extension = Path.GetExtension(uri.AbsolutePath);
        return extension.ToLowerInvariant() switch
        {
            ".png" => "png",
            ".jpg" => "jpg",
            ".jpeg" => "jpeg",
            ".webp" => "webp",
            _ => "png"
        };
    }

    private static string SanitizeFileNameSegment(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Trim().Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "ideogram" : sanitized;
    }

    private static async Task<string> FindAvailableGeneratedPathAsync(
        string outputDirectory,
        string baseFileName,
        string extensionToken,
        CancellationToken cancellationToken)
    {
        for (var copyIndex = 0; ; copyIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = copyIndex == 0
                ? $"{baseFileName}.{extensionToken}"
                : $"{baseFileName}_copy-{copyIndex:000}.{extensionToken}";

            var fullPath = Path.Combine(outputDirectory, fileName);
            if (!File.Exists(fullPath))
            {
                return fullPath;
            }

            await Task.Yield();
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
