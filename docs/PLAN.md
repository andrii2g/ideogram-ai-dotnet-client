# PLAN.md — Ideogram AI API v3 REST Client for .NET 10

## 1. Objective

Build a standalone, dependency-free .NET 10 REST client library for Ideogram AI API v3 and a companion console application for manual API testing.

The implementation must cover these Ideogram v3 endpoints:

1. `POST /v1/ideogram-v3/generate`
2. `POST /v1/ideogram-v3/generate-transparent`
3. `POST /v1/ideogram-v3/inpaint`
4. `POST /v1/ideogram-v3/remix`
5. `POST /v1/ideogram-v3/reframe`
6. `POST /v1/ideogram-v3/replace-background`

The client must use only the .NET Base Class Library:

- `HttpClient`
- `MultipartFormDataContent`
- `System.Text.Json`
- `System.Net.Http.Headers`
- `System.CommandLine` is **not allowed**
- `Newtonsoft.Json` is **not allowed**
- `Microsoft.Extensions.*` packages are **not allowed**
- no generated SDKs
- no unit tests
- no integration tests

Target framework: `net10.0`.

Recommended repository name: `ideogram-v3-dotnet-client`.

---

## 2. Source API references

Use these pages as the source of truth during implementation:

- https://developer.ideogram.ai/api-reference/api-reference/generate-v3
- https://developer.ideogram.ai/api-reference/api-reference/generate-transparent-v3
- https://developer.ideogram.ai/api-reference/api-reference/inpaint-v3
- https://developer.ideogram.ai/api-reference/api-reference/remix-v3
- https://developer.ideogram.ai/api-reference/api-reference/reframe-v3
- https://developer.ideogram.ai/api-reference/api-reference/replace-background-v3

General Ideogram API reference and setup pages:

- https://developer.ideogram.ai/ideogram-api/api-overview
- https://developer.ideogram.ai/ideogram-api/api-setup

Useful Ideogram generation-setting references:

- https://docs.ideogram.ai/using-ideogram/generation-settings/aspect-ratio-and-dimensions
- https://docs.ideogram.ai/using-ideogram/generation-settings/color-palette
- https://docs.ideogram.ai/using-ideogram/generation-settings/magic-prompt
- https://docs.ideogram.ai/using-ideogram/generation-settings/negative-prompt
- https://docs.ideogram.ai/using-ideogram/generation-settings/render-speed
- https://docs.ideogram.ai/using-ideogram/generation-settings/seed-number

---

## 3. Non-negotiable implementation constraints

1. Use `HttpClient` directly.
2. Do not add third-party NuGet packages.
3. All six Ideogram v3 endpoints must be represented as first-class async client methods.
4. Use `multipart/form-data` for all six endpoints.
5. Send the API key using the request header name `Api-Key` on Ideogram API requests only.
6. Never put `Api-Key` in `HttpClient.DefaultRequestHeaders`; add it per API request.
7. Do not send `Api-Key` or `Authorization` when downloading signed image URLs.
8. Do not log the raw API key.
9. Do not store the API key in source code.
10. Console app must allow manual execution of each endpoint by selecting a menu option.
11. Console app must read the API key from `IDEOGRAM_API_KEY`, command-line args, or a secure interactive prompt.
12. Image result URLs are temporary, so the console app must offer to download all returned images immediately.
13. API and image-download HTTP transports must be isolated. Do not reuse an API-configured `HttpClient` for signed image URL downloads.
14. `RequestTimeout` and `DownloadTimeout` must be enforced with per-operation cancellation; do not rely only on `HttpClient.Timeout`.
15. Generated download filename extensions must be normalized tokens without leading dots, for example `png`, not `.png`.
16. Existing output files must never be overwritten; generated downloads must use deterministic collision suffixes.
17. No test projects.
18. The solution must build cleanly with `dotnet build`.

### 3.1 Reviewer finding resolutions

These clarifications are mandatory implementation requirements and override any ambiguous wording elsewhere in this plan.

#### P1 — API-key isolation for signed-image downloads

- `Api-Key` must never be configured on `HttpClient.DefaultRequestHeaders`.
- Ideogram API calls must add `Api-Key` only to the individual `HttpRequestMessage` created for that API `POST`.
- Image downloads must use a dedicated headerless download HTTP path, either a separate internally owned `_downloadHttpClient` or an externally supplied download client validated to contain no credential defaults.
- The single-`HttpClient` constructor treats the supplied client as the API client only; it must create a separate private headerless client for downloads.
- The two-`HttpClient` constructor must reject the same instance being supplied for both API and download traffic.
- Generic signed-URL download helpers must not attach `Api-Key`, `Authorization`, or any other API credential header, even if a download URL happens to use the configured API host.

#### P1 — Download filename extension normalization

- Extension tokens are stored without leading dots: `png`, `jpg`, `jpeg`, `webp`.
- Filename templates append the dot themselves: `{prefix}_{index:00}_seed-{seed-or-na}.{extension}`.
- The implementation must never produce malformed names such as `generate_00_seed-123..png`.

#### P2 — Timeout enforcement

- `IdeogramClientOptions.RequestTimeout` and `IdeogramClientOptions.DownloadTimeout` must be wired into execution, not merely exposed as options.
- API calls must use a linked cancellation token source with `CancelAfter(RequestTimeout)` or an equivalent per-operation timeout mechanism.
- Downloads must use a linked cancellation token source with `CancelAfter(DownloadTimeout)` for the full lifecycle: headers, status validation, stream acquisition, file creation, copy, and flush/close.
- Caller cancellation has priority: caller-triggered cancellation propagates as `OperationCanceledException`; operation-timeout cancellation is converted to `TimeoutException` with the configured timeout value in the message.
- Do not rely only on `HttpClient.Timeout` for these semantics.

#### P2 — Output-file collision behavior

- Existing files must never be overwritten.
- `DownloadImageAsync(imageUrl, outputPath, ...)` treats `outputPath` as explicit and must fail with `IOException` when that path already exists.
- `DownloadImagesAsync(...)` uses generated deterministic names and resolves collisions with `_copy-{copyIndex:000}` before the extension, for example `generate_00_seed-123_copy-001.png`.
- File creation must use `FileMode.CreateNew` so the no-overwrite behavior is race-safe.

---

## 4. Repository layout

Create this structure:

```text
ideogram-v3-dotnet-client/
├─ IdeogramV3DotNet.slnx
├─ README.md
├─ docs/
│  └─ PLAN.md
├─ src/
│  └─ Ideogram.Client/
│     ├─ Ideogram.Client.csproj
│     ├─ IIdeogramClient.cs
│     ├─ IdeogramClient.cs
│     ├─ IdeogramClientOptions.cs
│     ├─ IdeogramApiException.cs
│     ├─ IdeogramFile.cs
│     ├─ Constants/
│     │  ├─ IdeogramAspectRatios.cs
│     │  ├─ IdeogramColorPalettes.cs
│     │  ├─ IdeogramMagicPrompt.cs
│     │  ├─ IdeogramRenderingSpeed.cs
│     │  ├─ IdeogramResolutions.cs
│     │  ├─ IdeogramStylePresets.cs
│     │  ├─ IdeogramStyleTypes.cs
│     │  └─ IdeogramUpscaleFactors.cs
│     ├─ Internal/
│     │  ├─ Guard.cs
│     │  ├─ HttpContentBuilder.cs
│     │  ├─ JsonDefaults.cs
│     │  └─ MimeTypeDetector.cs
│     ├─ Models/
│     │  ├─ IdeogramImageObject.cs
│     │  ├─ IdeogramResponse.cs
│     │  ├─ IdeogramErrorResponse.cs
│     │  ├─ ColorPalette.cs
│     │  ├─ ColorPaletteMember.cs
│     │  ├─ GenerateRequest.cs
│     │  ├─ GenerateTransparentRequest.cs
│     │  ├─ InpaintRequest.cs
│     │  ├─ RemixRequest.cs
│     │  ├─ ReframeRequest.cs
│     │  └─ ReplaceBackgroundRequest.cs
│     └─ Validation/
│        └─ RequestValidator.cs
└─ samples/
   └─ Ideogram.Client.Console/
      ├─ Ideogram.Client.Console.csproj
      ├─ Program.cs
      ├─ ConsolePrompts.cs
      ├─ ManualRunOptions.cs
      ├─ OutputWriter.cs
      └─ SimpleArgsParser.cs
```

Do not create `tests/`.

---

## 5. Project creation commands

Run these commands from the repository root:

```bash
dotnet new sln --name IdeogramV3DotNet

dotnet new classlib \
  --name Ideogram.Client \
  --output src/Ideogram.Client \
  --framework net10.0

dotnet new console \
  --name Ideogram.Client.Console \
  --output samples/Ideogram.Client.Console \
  --framework net10.0

dotnet sln IdeogramV3DotNet.slnx add src/Ideogram.Client/Ideogram.Client.csproj
dotnet sln IdeogramV3DotNet.slnx add samples/Ideogram.Client.Console/Ideogram.Client.Console.csproj
dotnet add samples/Ideogram.Client.Console/Ideogram.Client.Console.csproj reference src/Ideogram.Client/Ideogram.Client.csproj
```

Project file requirements for both projects:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <LangVersion>latest</LangVersion>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

Library project metadata:

```xml
<PropertyGroup>
  <PackageId>Ideogram.Client</PackageId>
  <Title>Ideogram API v3 .NET Client</Title>
  <Description>Dependency-free .NET 10 HttpClient wrapper for Ideogram AI API v3.</Description>
  <Authors>YourNameOrOrg</Authors>
  <RepositoryUrl>https://github.com/YOUR_ACCOUNT/ideogram-v3-dotnet-client</RepositoryUrl>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Only include package metadata if publishing to NuGet is desired. Do not add external package references.

---

## 6. API endpoint map

| Client method | HTTP method | Path | Content type | Required fields | Notes |
|---|---:|---|---|---|---|
| `GenerateAsync` | POST | `/v1/ideogram-v3/generate` | multipart/form-data | `prompt` | Text-to-image generation. Supports prompt, resolution/aspect ratio, style, palette, style references, character references. |
| `GenerateTransparentAsync` | POST | `/v1/ideogram-v3/generate-transparent` | multipart/form-data | `prompt` | Transparent-background generation. `rendering_speed=FLASH` is not supported. Supports `upscale_factor`. |
| `InpaintAsync` | POST | `/v1/ideogram-v3/inpaint` | multipart/form-data | `image`, `mask`, `prompt` | Inpaint/edit selected image regions. Mask must match image dimensions; API docs specify black regions indicate edit regions. |
| `RemixAsync` | POST | `/v1/ideogram-v3/remix` | multipart/form-data | `image`, `prompt` | Image-to-image remix. Supports `image_weight`. Input images are cropped to selected aspect ratio before remixing. |
| `ReframeAsync` | POST | `/v1/ideogram-v3/reframe` | multipart/form-data | `image`, `resolution` | Extends/reframes a square image to a selected resolution. |
| `ReplaceBackgroundAsync` | POST | `/v1/ideogram-v3/replace-background` | multipart/form-data | `image`, `prompt` | Keeps foreground subject and replaces background according to prompt. |

Base URL:

```text
https://api.ideogram.ai
```

Default API version path prefix is included per endpoint path, not as a separate option.

---

## 7. Public library API

### 7.1 `IIdeogramClient`

Create:

```csharp
namespace Ideogram.Client;

public interface IIdeogramClient
{
    Task<IdeogramResponse> GenerateAsync(
        GenerateRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> GenerateTransparentAsync(
        GenerateTransparentRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> InpaintAsync(
        InpaintRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> RemixAsync(
        RemixRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> ReframeAsync(
        ReframeRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> ReplaceBackgroundAsync(
        ReplaceBackgroundRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> DownloadImagesAsync(
        IdeogramResponse response,
        string outputDirectory,
        string fileNamePrefix = "ideogram",
        CancellationToken cancellationToken = default);

    Task<string> DownloadImageAsync(
        string imageUrl,
        string outputPath,
        CancellationToken cancellationToken = default);
}
```

### 7.2 `IdeogramClientOptions`

Create:

```csharp
namespace Ideogram.Client;

public sealed class IdeogramClientOptions
{
    public required string ApiKey { get; init; }

    public Uri BaseUri { get; init; } = new("https://api.ideogram.ai");

    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromMinutes(3);

    public TimeSpan DownloadTimeout { get; init; } = TimeSpan.FromMinutes(2);

    public string UserAgent { get; init; } = "Ideogram.Client/1.0 (+https://github.com/YOUR_ACCOUNT/ideogram-v3-dotnet-client)";
}
```

Validation:

- `ApiKey` must be non-empty.
- `BaseUri` must be absolute HTTPS.
- `RequestTimeout` and `DownloadTimeout` must be positive, non-zero, finite values.

Timeout semantics:

- `RequestTimeout` is the maximum duration for one Ideogram API `POST`, including request upload, response headers, and response body read.
- `DownloadTimeout` is the maximum duration for one image download `GET`, including response headers, response stream acquisition, file creation, file copy, and file flush/close.
- Enforce both timeout values with linked cancellation token sources or an equivalent per-operation timeout mechanism.
- Caller cancellation remains authoritative. If the caller token is canceled, propagate `OperationCanceledException` instead of converting it to `TimeoutException`.
- If the operation token is canceled while the caller token is not canceled, throw `TimeoutException` and include the configured timeout value in the message.
- Do not rely only on `HttpClient.Timeout`, because externally supplied clients may have arbitrary timeout settings.
- For internally created `HttpClient` instances, set `Timeout = Timeout.InfiniteTimeSpan` and enforce `RequestTimeout` and `DownloadTimeout` with linked cancellation token sources.
- For externally supplied `HttpClient` instances, do not mutate `Timeout`; a shorter caller-provided timeout may still cancel earlier and should be treated as a non-caller timeout.

### 7.3 `IdeogramClient`

Constructors:

```csharp
public sealed class IdeogramClient : IIdeogramClient, IDisposable
{
    public IdeogramClient(IdeogramClientOptions options);

    public IdeogramClient(HttpClient apiHttpClient, IdeogramClientOptions options, bool disposeApiHttpClient = false);

    public IdeogramClient(
        HttpClient apiHttpClient,
        HttpClient downloadHttpClient,
        IdeogramClientOptions options,
        bool disposeApiHttpClient = false,
        bool disposeDownloadHttpClient = false);
}
```

Required private state:

- `_options`
- `_apiKey`
- `_apiHttpClient`
- `_downloadHttpClient`
- ownership flags for each `HttpClient`

Constructor behavior:

- Store the API key in a private readonly field, for example `_apiKey`.
- Maintain two isolated HTTP paths:
  - `_apiHttpClient` for Ideogram API `POST` requests.
  - `_downloadHttpClient` for result image download `GET` requests.
- If the client creates its own API `HttpClient`, it owns and disposes it. Set its `Timeout` to `Timeout.InfiniteTimeSpan`.
- If the client creates its own download `HttpClient`, it owns and disposes it. Set its `Timeout` to `Timeout.InfiniteTimeSpan`.
- If an external API `HttpClient` is passed and `disposeApiHttpClient` is `false`, do not dispose it.
- If an external download `HttpClient` is passed and `disposeDownloadHttpClient` is `false`, do not dispose it.
- Do not mutate `Timeout` on externally supplied clients.
- The single-`HttpClient` constructor treats the supplied client as the API client only. Downloads must not use this instance; create a private headerless `_downloadHttpClient` for downloads.
- The two-`HttpClient` constructor must reject `ReferenceEquals(apiHttpClient, downloadHttpClient)` to keep the download transport isolated from API transport configuration.
- Reject any supplied API `HttpClient` whose `DefaultRequestHeaders` already contains `Api-Key`.
- Reject any supplied download `HttpClient` whose `DefaultRequestHeaders` contains a credential-bearing header; the download transport must be credential-free by default.
- Forbidden download default credential headers are case-insensitive exact names: `Api-Key`, `Authorization`, `X-Api-Key`, `X-API-Key`, and `X-Ideogram-Api-Key`. Do not use broad substring matching such as rejecting every header containing `key`; reject this documented list and `Authorization`.
- Never add `Api-Key` to `HttpClient.DefaultRequestHeaders` on any `HttpClient` instance.
- Do not set global `Content-Type`.
- For each multipart request, let `MultipartFormDataContent` create the boundary and content type.
- `User-Agent` may be set as a default request header on the API client because it is not secret. It may also be added per request.

Authentication design:

- API authentication is per request, not per `HttpClient`.
- `SendMultipartAsync` must add `Api-Key: <options.ApiKey>` to the `HttpRequestMessage` it creates for Ideogram API endpoints.
- Download helpers must use `_downloadHttpClient` and must not add `Api-Key` to download `HttpRequestMessage` instances.
- `DownloadImageAsync(string imageUrl, ...)` is a generic signed-URL downloader and must never infer authentication from arbitrary user-supplied URLs, even when the URL host matches `BaseUri`.
- Current Ideogram result URLs are signed and ephemeral, so the allowlist of authenticated download URLs is empty.
- If a future Ideogram endpoint documents an authenticated API-base file download, implement it as a separate explicit method with a hard-coded allowlist under `BaseUri`; do not change the generic download helper to attach credentials.

The two-`HttpClient` overload exists so callers can customize download transport without ever reusing the API-authenticated transport.


Required forbidden-download-header helper shape:

```csharp
private static readonly string[] ForbiddenDownloadDefaultHeaderNames =
[
    "Api-Key",
    "Authorization",
    "X-Api-Key",
    "X-API-Key",
    "X-Ideogram-Api-Key"
];

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
```

Use this helper in constructors that accept a download client and immediately before a download request is sent.

---

## 8. Shared models

### 8.1 `IdeogramResponse`

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ideogram.Client.Models;

public sealed class IdeogramResponse
{
    [JsonPropertyName("created")]
    public string? Created { get; init; }

    [JsonPropertyName("data")]
    public IReadOnlyList<IdeogramImageObject> Data { get; init; } = [];

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
```

Use `string? Created` rather than `DateTimeOffset` because the API sample uses a timestamp with a space between date and time. Add a helper later if strongly typed parsing is needed.

### 8.2 `IdeogramImageObject`

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ideogram.Client.Models;

public sealed class IdeogramImageObject
{
    [JsonPropertyName("prompt")]
    public string? Prompt { get; init; }

    [JsonPropertyName("resolution")]
    public string? Resolution { get; init; }

    [JsonPropertyName("is_image_safe")]
    public bool? IsImageSafe { get; init; }

    [JsonPropertyName("seed")]
    public int? Seed { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("style_type")]
    public string? StyleType { get; init; }

    [JsonPropertyName("upscaled_resolution")]
    public string? UpscaledResolution { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
```

Keep extension data for forward compatibility.

### 8.3 `IdeogramErrorResponse`

The exact error body can vary. Use a flexible model:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ideogram.Client.Models;

public sealed class IdeogramErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("error")]
    public JsonElement? Error { get; init; }

    [JsonPropertyName("detail")]
    public JsonElement? Detail { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtensionData { get; init; }
}
```

### 8.4 `IdeogramApiException`

```csharp
namespace Ideogram.Client;

public sealed class IdeogramApiException : HttpRequestException
{
    public HttpStatusCode StatusCodeValue { get; }
    public string? ResponseBody { get; }
    public string? RequestPath { get; }
    public string? RequestId { get; }

    public IdeogramApiException(
        HttpStatusCode statusCode,
        string message,
        string? responseBody,
        string? requestPath,
        string? requestId,
        Exception? innerException = null)
        : base(message, innerException, statusCode)
    {
        StatusCodeValue = statusCode;
        ResponseBody = responseBody;
        RequestPath = requestPath;
        RequestId = requestId;
    }
}
```

Do not include the API key in exception messages.

---

## 9. File abstraction

Create `IdeogramFile` so library users can pass files without exposing console-specific logic.

```csharp
namespace Ideogram.Client;

public sealed class IdeogramFile
{
    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required Func<Stream> OpenReadStream { get; init; }

    public long? Length { get; init; }

    public static IdeogramFile FromPath(string path, string? contentType = null)
    {
        // Validate path exists.
        // Use Path.GetFileName(path).
        // Infer content type from extension if not provided.
        // Set Length from new FileInfo(path).Length.
        // Open stream lazily with File.OpenRead(path).
    }
}
```

Supported input formats:

- `.jpg`
- `.jpeg`
- `.png`
- `.webp`

MIME mapping:

```text
.jpg  -> image/jpeg
.jpeg -> image/jpeg
.png  -> image/png
.webp -> image/webp
```

Maximum size:

- 10 MB for primary `image`
- 10 MB for `mask`
- 10 MB total across `style_reference_images`
- 10 MB total across `character_reference_images`
- 10 MB total across `character_reference_images_mask`

Use decimal megabytes unless a source specifically requires MiB:

```csharp
private const long MaxImageBytes = 10_000_000;
```

---

## 10. Request models

Request models should be mutable init-only classes, not positional records, to keep call sites readable.

### 10.1 Shared reusable option group

Do not create inheritance just to reuse properties if it makes JSON/multipart mapping unclear. Prefer duplicated properties or small internal helpers.

Common fields:

```csharp
public string? RenderingSpeed { get; init; }
public string? MagicPrompt { get; init; }
public string? NegativePrompt { get; init; }
public int? NumImages { get; init; }
public int? Seed { get; init; }
public ColorPalette? ColorPalette { get; init; }
public IReadOnlyList<string>? StyleCodes { get; init; }
public string? StyleType { get; init; }
public string? StylePreset { get; init; }
public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }
public IReadOnlyList<IdeogramFile>? CharacterReferenceImages { get; init; }
public IReadOnlyList<IdeogramFile>? CharacterReferenceImageMasks { get; init; }
```

### 10.2 `GenerateRequest`

Fields:

```csharp
public sealed class GenerateRequest
{
    public required string Prompt { get; init; }

    public int? Seed { get; init; }

    public string? Resolution { get; init; }

    public string? AspectRatio { get; init; }

    public string? RenderingSpeed { get; init; }

    public string? MagicPrompt { get; init; }

    public string? NegativePrompt { get; init; }

    public int? NumImages { get; init; }

    public ColorPalette? ColorPalette { get; init; }

    public IReadOnlyList<string>? StyleCodes { get; init; }

    public string? StyleType { get; init; }

    public string? StylePreset { get; init; }

    public string? CustomModelUri { get; init; }

    public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }

    public IReadOnlyList<IdeogramFile>? CharacterReferenceImages { get; init; }

    public IReadOnlyList<IdeogramFile>? CharacterReferenceImageMasks { get; init; }
}
```

Multipart field names:

```text
prompt
seed
resolution
aspect_ratio
rendering_speed
magic_prompt
negative_prompt
num_images
color_palette
style_codes
style_type
style_preset
custom_model_uri
style_reference_images
character_reference_images
character_reference_images_mask
```

### 10.3 `GenerateTransparentRequest`

Fields:

```csharp
public sealed class GenerateTransparentRequest
{
    public required string Prompt { get; init; }

    public int? Seed { get; init; }

    public string? UpscaleFactor { get; init; }

    public string? AspectRatio { get; init; }

    public string? RenderingSpeed { get; init; }

    public string? MagicPrompt { get; init; }

    public string? NegativePrompt { get; init; }

    public int? NumImages { get; init; }
}
```

Multipart field names:

```text
prompt
seed
upscale_factor
aspect_ratio
rendering_speed
magic_prompt
negative_prompt
num_images
```

Important validation:

- `RenderingSpeed` cannot be `FLASH`.
- Allowed transparent rendering speeds: `TURBO`, `DEFAULT`, `QUALITY`.
- `UpscaleFactor` allowed values: `X1`, `X2`, `X4`.

### 10.4 `InpaintRequest`

Fields:

```csharp
public sealed class InpaintRequest
{
    public required IdeogramFile Image { get; init; }

    public required IdeogramFile Mask { get; init; }

    public required string Prompt { get; init; }

    public string? MagicPrompt { get; init; }

    public int? NumImages { get; init; }

    public int? Seed { get; init; }

    public string? RenderingSpeed { get; init; }

    public string? StyleType { get; init; }

    public string? StylePreset { get; init; }

    public ColorPalette? ColorPalette { get; init; }

    public IReadOnlyList<string>? StyleCodes { get; init; }

    public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }

    public IReadOnlyList<IdeogramFile>? CharacterReferenceImages { get; init; }

    public IReadOnlyList<IdeogramFile>? CharacterReferenceImageMasks { get; init; }
}
```

Multipart field names:

```text
image
mask
prompt
magic_prompt
num_images
seed
rendering_speed
style_type
style_preset
color_palette
style_codes
style_reference_images
character_reference_images
character_reference_images_mask
```

Do not attempt to verify image and mask dimensions with image libraries because no third-party packages are allowed. Validate file existence, format, and size. Let the API enforce dimension matching.

### 10.5 `RemixRequest`

Fields:

```csharp
public sealed class RemixRequest
{
    public required IdeogramFile Image { get; init; }

    public required string Prompt { get; init; }

    public int? ImageWeight { get; init; }

    public int? Seed { get; init; }

    public string? Resolution { get; init; }

    public string? AspectRatio { get; init; }

    public string? RenderingSpeed { get; init; }

    public string? MagicPrompt { get; init; }

    public string? NegativePrompt { get; init; }

    public int? NumImages { get; init; }

    public ColorPalette? ColorPalette { get; init; }

    public IReadOnlyList<string>? StyleCodes { get; init; }

    public string? StyleType { get; init; }

    public string? StylePreset { get; init; }

    public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }

    public IReadOnlyList<IdeogramFile>? CharacterReferenceImages { get; init; }

    public IReadOnlyList<IdeogramFile>? CharacterReferenceImageMasks { get; init; }
}
```

Multipart field names:

```text
image
prompt
image_weight
seed
resolution
aspect_ratio
rendering_speed
magic_prompt
negative_prompt
num_images
color_palette
style_codes
style_type
style_preset
style_reference_images
character_reference_images
character_reference_images_mask
```

Important validation:

- `ImageWeight` allowed range: 1 through 100.
- `Resolution` and `AspectRatio` are mutually exclusive.

### 10.6 `ReframeRequest`

Fields:

```csharp
public sealed class ReframeRequest
{
    public required IdeogramFile Image { get; init; }

    public required string Resolution { get; init; }

    public int? NumImages { get; init; }

    public int? Seed { get; init; }

    public string? RenderingSpeed { get; init; }

    public string? StylePreset { get; init; }

    public ColorPalette? ColorPalette { get; init; }

    public IReadOnlyList<string>? StyleCodes { get; init; }

    public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }
}
```

Multipart field names:

```text
image
resolution
num_images
seed
rendering_speed
style_preset
color_palette
style_codes
style_reference_images
```

Important validation:

- `Resolution` is required.
- `Resolution` must be one of known Ideogram v3 resolutions.

### 10.7 `ReplaceBackgroundRequest`

Fields:

```csharp
public sealed class ReplaceBackgroundRequest
{
    public required IdeogramFile Image { get; init; }

    public required string Prompt { get; init; }

    public string? MagicPrompt { get; init; }

    public int? NumImages { get; init; }

    public int? Seed { get; init; }

    public string? RenderingSpeed { get; init; }

    public string? StylePreset { get; init; }

    public ColorPalette? ColorPalette { get; init; }

    public IReadOnlyList<string>? StyleCodes { get; init; }

    public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }
}
```

Multipart field names:

```text
image
prompt
magic_prompt
num_images
seed
rendering_speed
style_preset
color_palette
style_codes
style_reference_images
```

---

## 11. Constants

Use string constants rather than C# enums because API values include values such as `80S_ILLUSTRATION`, `1024x1024`, and aspect ratio strings.

### 11.1 Rendering speed constants

```csharp
public static class IdeogramRenderingSpeed
{
    public const string Flash = "FLASH";
    public const string Turbo = "TURBO";
    public const string Default = "DEFAULT";
    public const string Quality = "QUALITY";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Flash, Turbo, Default, Quality
    };

    public static readonly IReadOnlySet<string> TransparentAllowed = new HashSet<string>(StringComparer.Ordinal)
    {
        Turbo, Default, Quality
    };
}
```

### 11.2 Magic Prompt constants

```csharp
public static class IdeogramMagicPrompt
{
    public const string Auto = "AUTO";
    public const string On = "ON";
    public const string Off = "OFF";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Auto, On, Off
    };
}
```

### 11.3 Style type constants

```csharp
public static class IdeogramStyleTypes
{
    public const string Auto = "AUTO";
    public const string General = "GENERAL";
    public const string Realistic = "REALISTIC";
    public const string Design = "DESIGN";
    public const string Fiction = "FICTION";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Auto, General, Realistic, Design, Fiction
    };
}
```

### 11.4 Upscale factor constants

```csharp
public static class IdeogramUpscaleFactors
{
    public const string X1 = "X1";
    public const string X2 = "X2";
    public const string X4 = "X4";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        X1, X2, X4
    };
}
```

### 11.5 Color palette constants

Known preset names:

```csharp
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
        Ember, Fresh, Jungle, Magic, Melon, Mosaic, Pastel, Ultramarine
    };
}
```

### 11.6 Aspect ratio constants

Ideogram UI documentation uses colon notation such as `16:9`; API reference text indicates API values such as `1x1`. Implement API constants with `x` notation and normalize console input by accepting either `16:9` or `16x9`.

Known v3 aspect ratios:

```csharp
public static class IdeogramAspectRatios
{
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

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
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
        return value.Trim().Replace(':', 'x');
    }
}
```

### 11.7 Resolution constants

Known v3 resolution strings:

```csharp
public static class IdeogramResolutions
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        "1536x512",
        "1536x576",
        "1472x576",
        "1408x576",
        "1536x640",
        "1472x640",
        "1408x640",
        "1344x640",
        "1472x704",
        "1408x704",
        "1344x704",
        "1280x704",
        "1312x736",
        "1344x768",
        "1216x704",
        "1280x768",
        "1152x704",
        "1280x800",
        "1216x768",
        "1248x832",
        "1216x832",
        "1088x768",
        "1152x832",
        "1152x864",
        "1088x832",
        "1152x896",
        "1120x896",
        "1024x832",
        "1088x896",
        "960x832",
        "1024x896",
        "1088x960",
        "960x896",
        "1024x960",
        "1024x1024",
        "960x1024",
        "896x960",
        "960x1088",
        "896x1024",
        "832x960",
        "896x1088",
        "832x1024",
        "896x1120",
        "896x1152",
        "832x1088",
        "864x1152",
        "832x1152",
        "768x1088",
        "832x1216",
        "832x1248",
        "768x1216",
        "800x1280",
        "704x1152",
        "768x1280",
        "704x1216",
        "768x1344",
        "736x1312",
        "704x1280",
        "704x1344",
        "704x1408",
        "704x1472",
        "640x1344",
        "640x1408",
        "640x1472",
        "640x1536",
        "576x1408",
        "576x1472",
        "576x1536",
        "512x1536"
    };
}
```

### 11.8 Style preset constants

Use a string set so the validator can warn about typos while still allowing the codebase to be updated easily if Ideogram adds values.

```csharp
public static class IdeogramStylePresets
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        "80S_ILLUSTRATION",
        "90S_NOSTALGIA",
        "ABSTRACT_ORGANIC",
        "ANALOG_NOSTALGIA",
        "ART_BRUT",
        "ART_DECO",
        "ART_POSTER",
        "AURA",
        "AVANT_GARDE",
        "BAUHAUS",
        "BLUEPRINT",
        "BLURRY_MOTION",
        "BRIGHT_ART",
        "C4D_CARTOON",
        "CHILDRENS_BOOK",
        "COLLAGE",
        "COLORING_BOOK_I",
        "COLORING_BOOK_II",
        "CUBISM",
        "DARK_AURA",
        "DOODLE",
        "DOUBLE_EXPOSURE",
        "DRAMATIC_CINEMA",
        "EDITORIAL",
        "EMOTIONAL_MINIMAL",
        "ETHEREAL_PARTY",
        "EXPIRED_FILM",
        "FLAT_ART",
        "FLAT_VECTOR",
        "FOREST_REVERIE",
        "GEO_MINIMALIST",
        "GLASS_PRISM",
        "GOLDEN_HOUR",
        "GRAFFITI_I",
        "GRAFFITI_II",
        "HALFTONE_PRINT",
        "HIGH_CONTRAST",
        "HIPPIE_ERA",
        "ICONIC",
        "JAPANDI_FUSION",
        "JAZZY",
        "LONG_EXPOSURE",
        "MAGAZINE_EDITORIAL",
        "MINIMAL_ILLUSTRATION",
        "MIXED_MEDIA",
        "MONOCHROME",
        "NIGHTLIFE",
        "OIL_PAINTING",
        "OLD_CARTOONS",
        "PAINT_GESTURE",
        "POP_ART",
        "RETRO_ETCHING",
        "RIVIERA_POP",
        "SPOTLIGHT_80S",
        "STYLIZED_RED",
        "SURREAL_COLLAGE",
        "TRAVEL_POSTER",
        "VINTAGE_GEO",
        "VINTAGE_POSTER",
        "WATERCOLOR",
        "WEIRD",
        "WOODBLOCK_PRINT"
    };
}
```

---

## 12. Color palette model

Ideogram supports either a preset name or custom members. Model this as a single class with validation requiring exactly one mode.

```csharp
using System.Text.Json.Serialization;

namespace Ideogram.Client.Models;

public sealed class ColorPalette
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("members")]
    public IReadOnlyList<ColorPaletteMember>? Members { get; init; }

    public static ColorPalette FromPreset(string name)
    {
        return new ColorPalette { Name = name };
    }

    public static ColorPalette FromMembers(params ColorPaletteMember[] members)
    {
        return new ColorPalette { Members = members };
    }
}

public sealed class ColorPaletteMember
{
    [JsonPropertyName("color_hex")]
    public required string ColorHex { get; init; }

    [JsonPropertyName("color_weight")]
    public double? ColorWeight { get; init; }
}
```

Validation:

- Exactly one of `Name` or `Members` must be set.
- `Name`, when set, should be one of known palette constants.
- `Members`, when set, must contain 1 to 5 members.
- `ColorHex` must match `^#[0-9A-Fa-f]{6}$`.
- `ColorWeight`, when set, should be `0 <= weight <= 1`.

Multipart serialization:

- Add one form field named `color_palette`.
- Field value is compact JSON generated by `System.Text.Json`.
- Example preset field value:

```json
{"name":"EMBER"}
```

- Example custom field value:

```json
{"members":[{"color_hex":"#000000","color_weight":0.7},{"color_hex":"#f1c859","color_weight":0.3}]}
```

Keep this serialization isolated in `HttpContentBuilder.AddColorPalette(...)`. If manual testing shows Ideogram expects another multipart object encoding, only this helper should need adjustment.

---

## 13. Multipart form builder

Create `Internal/HttpContentBuilder.cs`.

Responsibilities:

1. Add required and optional string fields.
2. Add optional integer fields using invariant culture.
3. Add repeated string fields for list fields.
4. Add file fields.
5. Add object fields as compact JSON string fields.

Required helper methods:

```csharp
internal static class HttpContentBuilder
{
    public static void AddRequiredString(
        MultipartFormDataContent content,
        string name,
        string value);

    public static void AddOptionalString(
        MultipartFormDataContent content,
        string name,
        string? value);

    public static void AddOptionalInt(
        MultipartFormDataContent content,
        string name,
        int? value);

    public static void AddRepeatedStrings(
        MultipartFormDataContent content,
        string name,
        IReadOnlyList<string>? values);

    public static void AddFile(
        MultipartFormDataContent content,
        string name,
        IdeogramFile file);

    public static void AddFiles(
        MultipartFormDataContent content,
        string name,
        IReadOnlyList<IdeogramFile>? files);

    public static void AddColorPalette(
        MultipartFormDataContent content,
        ColorPalette? colorPalette);
}
```

Implementation notes:

- Use `StringContent(value, Encoding.UTF8)`.
- For `StreamContent`, set `Headers.ContentType = new MediaTypeHeaderValue(file.ContentType)`.
- Use `content.Add(streamContent, name, file.FileName)`.
- For `style_codes`, add repeated string fields using the same field name.
- For multiple files, repeat the exact same field name; do not append `[]`.
- `MultipartFormDataContent` disposal should dispose `StreamContent` and file streams.

Do **not** do this:

```csharp
httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
```

The boundary must be generated by `MultipartFormDataContent`.

---

## 14. Request validation

Create `Validation/RequestValidator.cs`.

### 14.1 Common validation rules

Implement:

```csharp
internal static class RequestValidator
{
    public static void Validate(GenerateRequest request);
    public static void Validate(GenerateTransparentRequest request);
    public static void Validate(InpaintRequest request);
    public static void Validate(RemixRequest request);
    public static void Validate(ReframeRequest request);
    public static void Validate(ReplaceBackgroundRequest request);
}
```

Rules:

- Required prompt:
  - non-null
  - not empty
  - not whitespace
- Seed:
  - `0 <= seed <= 2147483647`
- NumImages:
  - `1 <= num_images <= 8`
- RenderingSpeed:
  - must be in known set
  - transparent endpoint must not allow `FLASH`
- MagicPrompt:
  - must be `AUTO`, `ON`, or `OFF`
- Resolution:
  - must be in known resolution set
- AspectRatio:
  - normalize `:` to `x`
  - must be in known aspect ratio set
- Generate and Remix:
  - `Resolution` and `AspectRatio` are mutually exclusive
- StyleType:
  - must be in known style type set
- StylePreset:
  - must be in known style preset set
- StyleCodes:
  - each code must match `^[0-9A-Fa-f]{8}$`
  - cannot be used with `StyleReferenceImages`
  - cannot be used with `StyleType` for request types that expose `StyleType`
- ImageWeight:
  - `1 <= image_weight <= 100`
- Files:
  - file name must be present
  - supported content type
  - supported extension for file-path inputs
  - size must be known and <= 10 MB where applicable
- Character reference masks:
  - if `CharacterReferenceImageMasks` is set, `CharacterReferenceImages` must also be set
  - mask count must match character reference image count
- ColorPalette:
  - exactly one of preset name or custom members
  - custom members length 1 to 5
  - hex and weight validation

### 14.2 Error style

Throw `ArgumentException`, `ArgumentOutOfRangeException`, or `InvalidOperationException` before sending invalid requests.

Message examples:

```text
GenerateRequest.Prompt is required.
GenerateRequest.Resolution and GenerateRequest.AspectRatio cannot both be set.
GenerateTransparentRequest.RenderingSpeed cannot be FLASH.
RemixRequest.ImageWeight must be between 1 and 100.
ColorPalette must specify either Name or Members, not both.
```

Do not send invalid requests to the API when they can be detected locally.

---

## 15. HTTP request execution

Create one private send method in `IdeogramClient`.

```csharp
private async Task<IdeogramResponse> SendMultipartAsync(
    string relativePath,
    MultipartFormDataContent content,
    CancellationToken cancellationToken)
```

The send method must build an absolute API URI from `IdeogramClientOptions.BaseUri` and `relativePath`. Do not depend on `HttpClient.BaseAddress` being configured by the caller.

Implementation requirements:

1. Create a linked cancellation token source from the caller token.
2. Apply `options.RequestTimeout` with `CancelAfter(options.RequestTimeout)`.
3. Create `HttpRequestMessage(HttpMethod.Post, BuildApiUri(relativePath))`.
4. Set `Content = content`.
5. Verify `_apiHttpClient.DefaultRequestHeaders` does not contain `Api-Key` before sending.
6. Add `Api-Key: <api key>` to `request.Headers` for this request only.
7. Add `User-Agent` to either `request.Headers` or the API client's default headers.
8. Use `_apiHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationToken)`.
9. Use the linked timeout token, not the original caller token, for all response-body reads in this method.
10. Read response body as string.
11. On non-success status:
    - parse best-effort error message
    - throw `IdeogramApiException`
    - include status code
    - include raw response body
    - include request path
    - include response request id header if present
12. On success:
    - deserialize `IdeogramResponse`
    - if deserialization fails, throw `IdeogramApiException` or `InvalidOperationException` with raw body and inner exception
13. Never swallow caller cancellation. If `cancellationToken.IsCancellationRequested` is true, let `OperationCanceledException` propagate.
14. If an operation is canceled while the caller token is not canceled, throw `TimeoutException` with the request path and configured timeout value.
15. Do not rely on `HttpClient.Timeout` to implement `RequestTimeout`.

Required timeout and authentication shape:

```csharp
using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
timeoutCts.CancelAfter(_options.RequestTimeout);
var operationToken = timeoutCts.Token;

try
{
    if (_apiHttpClient.DefaultRequestHeaders.Contains("Api-Key"))
    {
        throw new InvalidOperationException("Do not configure Api-Key in HttpClient.DefaultRequestHeaders; Ideogram API authentication is added per request only.");
    }

    using var request = new HttpRequestMessage(HttpMethod.Post, BuildApiUri(relativePath));

    if (!request.Headers.TryAddWithoutValidation("Api-Key", _apiKey))
    {
        throw new InvalidOperationException("Could not add the Ideogram Api-Key request header.");
    }

    request.Content = content;

    using var response = await _apiHttpClient
        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, operationToken)
        .ConfigureAwait(false);

    var body = await response.Content
        .ReadAsStringAsync(operationToken)
        .ConfigureAwait(false);

    // Continue status handling and JSON deserialization.
}
catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
{
    throw new TimeoutException($"Ideogram API request to '{relativePath}' exceeded {_options.RequestTimeout}.", ex);
}
```

Suggested parse logic:

```csharp
private static string BuildErrorMessage(HttpStatusCode statusCode, string body)
{
    // Try deserialize IdeogramErrorResponse.
    // Prefer Message.
    // Else prefer Detail as raw JSON.
    // Else prefer Error as raw JSON.
    // Else use body.
}
```

---

## 16. Client method implementation details

### 16.1 `GenerateAsync`

Pseudo-code:

```csharp
public async Task<IdeogramResponse> GenerateAsync(
    GenerateRequest request,
    CancellationToken cancellationToken = default)
{
    RequestValidator.Validate(request);

    using var content = new MultipartFormDataContent();

    HttpContentBuilder.AddRequiredString(content, "prompt", request.Prompt);
    HttpContentBuilder.AddOptionalInt(content, "seed", request.Seed);
    HttpContentBuilder.AddOptionalString(content, "resolution", request.Resolution);
    HttpContentBuilder.AddOptionalString(content, "aspect_ratio", NormalizeAspectRatioOrNull(request.AspectRatio));
    HttpContentBuilder.AddOptionalString(content, "rendering_speed", request.RenderingSpeed);
    HttpContentBuilder.AddOptionalString(content, "magic_prompt", request.MagicPrompt);
    HttpContentBuilder.AddOptionalString(content, "negative_prompt", request.NegativePrompt);
    HttpContentBuilder.AddOptionalInt(content, "num_images", request.NumImages);
    HttpContentBuilder.AddColorPalette(content, request.ColorPalette);
    HttpContentBuilder.AddRepeatedStrings(content, "style_codes", request.StyleCodes);
    HttpContentBuilder.AddOptionalString(content, "style_type", request.StyleType);
    HttpContentBuilder.AddOptionalString(content, "style_preset", request.StylePreset);
    HttpContentBuilder.AddOptionalString(content, "custom_model_uri", request.CustomModelUri);
    HttpContentBuilder.AddFiles(content, "style_reference_images", request.StyleReferenceImages);
    HttpContentBuilder.AddFiles(content, "character_reference_images", request.CharacterReferenceImages);
    HttpContentBuilder.AddFiles(content, "character_reference_images_mask", request.CharacterReferenceImageMasks);

    return await SendMultipartAsync("/v1/ideogram-v3/generate", content, cancellationToken)
        .ConfigureAwait(false);
}
```

### 16.2 `GenerateTransparentAsync`

Use path:

```text
/v1/ideogram-v3/generate-transparent
```

Add fields:

```text
prompt
seed
upscale_factor
aspect_ratio
rendering_speed
magic_prompt
negative_prompt
num_images
```

Validation must reject `FLASH`.

### 16.3 `InpaintAsync`

Use path:

```text
/v1/ideogram-v3/inpaint
```

Add fields:

```text
image
mask
prompt
magic_prompt
num_images
seed
rendering_speed
style_type
style_preset
color_palette
style_codes
style_reference_images
character_reference_images
character_reference_images_mask
```

### 16.4 `RemixAsync`

Use path:

```text
/v1/ideogram-v3/remix
```

Add fields:

```text
image
prompt
image_weight
seed
resolution
aspect_ratio
rendering_speed
magic_prompt
negative_prompt
num_images
color_palette
style_codes
style_type
style_preset
style_reference_images
character_reference_images
character_reference_images_mask
```

### 16.5 `ReframeAsync`

Use path:

```text
/v1/ideogram-v3/reframe
```

Add fields:

```text
image
resolution
num_images
seed
rendering_speed
style_preset
color_palette
style_codes
style_reference_images
```

### 16.6 `ReplaceBackgroundAsync`

Use path:

```text
/v1/ideogram-v3/replace-background
```

Add fields:

```text
image
prompt
magic_prompt
num_images
seed
rendering_speed
style_preset
color_palette
style_codes
style_reference_images
```

---

## 17. Image download helpers

Because Ideogram image URLs are temporary, implement optional download helpers.

### 17.1 `DownloadImageAsync`

Behavior:

1. Validate `imageUrl` is absolute HTTP or HTTPS.
2. Use the dedicated `_downloadHttpClient`, not the API `HttpClient` instance.
3. Create a linked cancellation token source from the caller token.
4. Apply `options.DownloadTimeout` with `CancelAfter(options.DownloadTimeout)`.
5. Create `HttpRequestMessage(HttpMethod.Get, imageUri)`.
6. Do not add `Api-Key`, `Authorization`, or any other API authentication header to the download request.
7. Verify `_downloadHttpClient.DefaultRequestHeaders` contains none of the forbidden download default credential headers from section 7.3 before sending.
8. Generic download helpers must never infer authentication from `imageUrl`, even if the host matches `IdeogramClientOptions.BaseUri`.
9. Send the request through `_downloadHttpClient` with `ResponseHeadersRead`.
10. Use the linked timeout token for the full download lifecycle: response headers, status check, response stream acquisition, file creation, stream copy, and file flush/close.
11. Ensure success status.
12. Create output directory if missing.
13. If the exact `outputPath` already exists, throw `IOException`. `DownloadImageAsync` receives an explicit path and must never overwrite it or silently rename it.
14. Stream response to file using `FileMode.CreateNew` to make collision behavior race-safe.
15. Return final output path.

Required timeout and download shape:

```csharp
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

    await using var input = await response.Content
        .ReadAsStreamAsync(operationToken)
        .ConfigureAwait(false);

    await using var output = new FileStream(
        finalPath,
        FileMode.CreateNew,
        FileAccess.Write,
        FileShare.None,
        bufferSize: 81920,
        useAsync: true);

    await input.CopyToAsync(output, operationToken).ConfigureAwait(false);
    await output.FlushAsync(operationToken).ConfigureAwait(false);
}
catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
{
    throw new TimeoutException($"Ideogram image download exceeded {_options.DownloadTimeout}: {imageUri}", ex);
}
```

### 17.2 Extension normalization

Determine the local file extension from the URL path only. Ignore query-string values.

Required behavior:

1. Read `Path.GetExtension(imageUri.AbsolutePath)`.
2. Remove the leading dot.
3. Normalize with `ToLowerInvariant()`.
4. Allow only these extension tokens, without leading dots:
   - `png`
   - `jpg`
   - `jpeg`
   - `webp`
5. Use fallback extension token `png` when the path has no extension or an unsupported extension.

The filename template appends the dot itself, so `extension` must never include a leading dot.


Required helper shape:

```csharp
private static string NormalizeDownloadExtension(Uri imageUri)
{
    var raw = Path.GetExtension(imageUri.AbsolutePath);
    var token = string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.TrimStart('.');
    token = token.ToLowerInvariant();

    return token is "png" or "jpg" or "jpeg" or "webp" ? token : "png";
}
```

The helper returns an extension token such as `png`, never `.png`.

### 17.3 Output collision behavior

Do not overwrite existing files.

Required behavior:

- `DownloadImageAsync(imageUrl, outputPath, ...)` treats `outputPath` as an explicit caller-selected path.
- If `outputPath` already exists, throw `IOException` before writing or use `FileMode.CreateNew` so the operation fails without truncating the existing file.
- `DownloadImagesAsync(...)` creates generated names and must avoid collisions by appending a deterministic numeric suffix before the extension.
- Collision suffix pattern: `_copy-{copyIndex:000}`.
- Example: `generate_00_seed-123.png`, then `generate_00_seed-123_copy-001.png` if the first path already exists.
- Return the actual saved paths after collision resolution.
- The console app uses timestamped output directories, but it must still use the same collision-safe helper.
- Use `FileMode.CreateNew` when opening the final target. If another process creates the same file between path selection and file creation, retry with the next collision suffix for generated paths.

### 17.4 `DownloadImagesAsync`

Behavior:

1. Iterate over `response.Data`.
2. Skip objects with null/empty URL.
3. Determine normalized extension from URL path using section 17.2.
4. Primary file naming pattern:

```text
{fileNamePrefix}_{index:00}_seed-{seed-or-na}.{extension}
```

5. Collision file naming pattern:

```text
{fileNamePrefix}_{index:00}_seed-{seed-or-na}_copy-{copyIndex:000}.{extension}
```

6. Use only local safe names generated by the client; do not trust remote file names beyond the sanitized extension.
7. Resolve collisions using section 17.3.
8. Download each image through the same headerless `_downloadHttpClient` path as `DownloadImageAsync`.
9. Return list of actual saved paths.

---

## 18. Console application

The console app is a manual tester, not a polished CLI framework.

### 18.1 Startup behavior

Order of API key resolution:

1. `--api-key <key>` command-line option.
2. `IDEOGRAM_API_KEY` environment variable.
3. Interactive hidden prompt.

Do not print the API key.

Suggested hidden prompt implementation:

```csharp
private static string ReadSecret(string prompt)
{
    Console.Write(prompt);
    var chars = new List<char>();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (chars.Count > 0)
            {
                chars.RemoveAt(chars.Count - 1);
                Console.Write("\b \b");
            }
            continue;
        }

        chars.Add(key.KeyChar);
        Console.Write("*");
    }

    return new string(chars.ToArray());
}
```

### 18.2 Menu

Print:

```text
Ideogram API v3 Manual Console

API key: loaded
Output directory: ./outputs/yyyyMMdd-HHmmss

Select method:
  1) Generate
  2) Generate transparent
  3) Inpaint
  4) Remix
  5) Reframe
  6) Replace background
  7) Download image by URL
  0) Exit
```

After each run:

- print pretty JSON response
- write response JSON to output directory
- ask:

```text
Download returned images now? [Y/n]:
```

Default should be `Y`.

### 18.3 Shared prompt helpers

Create `ConsolePrompts.cs`:

```csharp
internal static class ConsolePrompts
{
    public static string RequiredString(string label);
    public static string? OptionalString(string label, string? defaultValue = null);
    public static int? OptionalInt(string label, int? defaultValue = null, int? min = null, int? max = null);
    public static bool Confirm(string label, bool defaultValue = true);
    public static string? OptionalPath(string label);
    public static IdeogramFile RequiredImageFile(string label);
    public static IReadOnlyList<IdeogramFile>? OptionalImageFiles(string label);
    public static IReadOnlyList<string>? OptionalStringList(string label);
}
```

For optional lists:

- Ask for semicolon-separated values.
- Empty input means null.
- Example:

```text
Style codes, separated by semicolon, blank to skip:
```

For optional files:

- Ask for paths separated by semicolon.
- Trim quotes around paths.

### 18.4 Per-method manual forms

#### Generate menu prompts

Required:

- `prompt`

Optional:

- `seed`
- choose `resolution` or `aspect_ratio`
- `rendering_speed`
- `magic_prompt`
- `negative_prompt`
- `num_images`
- `style_type`
- `style_preset`
- `custom_model_uri`
- `style_codes`
- `color_palette`
- `style_reference_images`
- `character_reference_images`
- `character_reference_images_mask`

Defaults:

```text
rendering_speed = TURBO
num_images = 1
style_type = GENERAL
```

#### Generate transparent menu prompts

Required:

- `prompt`

Optional:

- `seed`
- `upscale_factor`
- `aspect_ratio`
- `rendering_speed`
- `magic_prompt`
- `negative_prompt`
- `num_images`

Defaults:

```text
rendering_speed = TURBO
upscale_factor = X1
num_images = 1
```

Do not offer `FLASH`.

#### Inpaint menu prompts

Required:

- `image path`
- `mask path`
- `prompt`

Optional:

- `magic_prompt`
- `num_images`
- `seed`
- `rendering_speed`
- `style_type`
- `style_preset`
- `color_palette`
- `style_codes`
- `style_reference_images`
- `character_reference_images`
- `character_reference_images_mask`

Default:

```text
rendering_speed = DEFAULT
num_images = 1
```

Print a reminder:

```text
Mask must be the same dimensions as the image. Per Ideogram docs, black mask regions indicate the regions to edit.
```

#### Remix menu prompts

Required:

- `image path`
- `prompt`

Optional:

- `image_weight`
- `seed`
- `resolution` or `aspect_ratio`
- `rendering_speed`
- `magic_prompt`
- `negative_prompt`
- `num_images`
- `color_palette`
- `style_codes`
- `style_type`
- `style_preset`
- `style_reference_images`
- `character_reference_images`
- `character_reference_images_mask`

Defaults:

```text
image_weight = 50
rendering_speed = TURBO
num_images = 1
style_type = GENERAL
```

#### Reframe menu prompts

Required:

- `image path`
- `resolution`

Optional:

- `num_images`
- `seed`
- `rendering_speed`
- `style_preset`
- `color_palette`
- `style_codes`
- `style_reference_images`

Defaults:

```text
resolution = 1280x768 or ask user
rendering_speed = DEFAULT
num_images = 1
```

Do not silently force a resolution if user wants another value. Show available common examples:

```text
Common v3 resolutions:
  1024x1024
  1312x736
  736x1312
  1280x800
  800x1280
  1536x512
  512x1536
```

#### Replace background menu prompts

Required:

- `image path`
- `prompt`

Optional:

- `magic_prompt`
- `num_images`
- `seed`
- `rendering_speed`
- `style_preset`
- `color_palette`
- `style_codes`
- `style_reference_images`

Defaults:

```text
rendering_speed = DEFAULT
num_images = 1
```

### 18.5 Lightweight command-line mode

Implement a minimal parser without packages.

Examples:

```bash
dotnet run --project samples/Ideogram.Client.Console -- generate \
  --prompt "A photo of a cat sleeping on a couch." \
  --rendering-speed TURBO \
  --num-images 1 \
  --download true

dotnet run --project samples/Ideogram.Client.Console -- transparent \
  --prompt "A clean logo for Ideogram Coffee" \
  --aspect-ratio 1x1 \
  --upscale-factor X2

dotnet run --project samples/Ideogram.Client.Console -- inpaint \
  --image ./input/cat.png \
  --mask ./input/mask.png \
  --prompt "A photo of a cat wearing a hat."

dotnet run --project samples/Ideogram.Client.Console -- remix \
  --image ./input/cat.png \
  --prompt "A photo of a dog sleeping on a couch" \
  --image-weight 60

dotnet run --project samples/Ideogram.Client.Console -- reframe \
  --image ./input/square.png \
  --resolution 1312x736

dotnet run --project samples/Ideogram.Client.Console -- replace-background \
  --image ./input/person.png \
  --prompt "A busy coffee shop in the background"
```

If parsing fails, fall back to interactive menu and print a short help message.

Do not implement a full CLI framework.

---

## 19. Output behavior

Create output directory:

```text
outputs/yyyyMMdd-HHmmss/
```

For each API call:

1. Save raw pretty response JSON:

```text
outputs/yyyyMMdd-HHmmss/{method}_response.json
```

2. Save images when downloading using extension normalization and collision behavior from section 17:

```text
outputs/yyyyMMdd-HHmmss/{method}_{index:00}_seed-{seed-or-na}.{extension}
outputs/yyyyMMdd-HHmmss/{method}_{index:00}_seed-{seed-or-na}_copy-{copyIndex:000}.{extension}
```

The first image-name form is used when no file exists. The `_copy-{copyIndex:000}` form is used only when the primary generated path already exists. `extension` is the normalized extension token without a leading dot, usually `png`.

3. Print saved paths.
4. Never overwrite an existing output file.

Example console output:

```text
Request succeeded.
Created: 2000-01-23 04:56:07+00:00
Images returned: 1

[0]
  Safe: true
  Seed: 12345
  Resolution: 1024x1024
  URL: https://ideogram.ai/api/images/ephemeral/...

Saved response:
  outputs/20260505-143501/generate_response.json

Saved images:
  outputs/20260505-143501/generate_00_seed-12345.png
```

---

## 20. README content

Create a concise `README.md` with:

1. Project description.
2. Supported endpoints.
3. No third-party package statement.
4. Installation/build commands.
5. API key setup.
6. Minimal library usage example.
7. Console usage examples.
8. Notes:
   - image URLs expire
   - all v3 methods use multipart
   - transparent endpoint does not support `FLASH`
   - no tests included by request

### 20.1 Minimal library example

```csharp
using Ideogram.Client;
using Ideogram.Client.Models;

var client = new IdeogramClient(new IdeogramClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("IDEOGRAM_API_KEY")
        ?? throw new InvalidOperationException("IDEOGRAM_API_KEY is not set.")
});

var response = await client.GenerateAsync(new GenerateRequest
{
    Prompt = "A photo of a cat sleeping on a couch.",
    RenderingSpeed = IdeogramRenderingSpeed.Turbo,
    NumImages = 1
});

await client.DownloadImagesAsync(response, "outputs", "generate");
// If a generated output file already exists, the client saves with a deterministic _copy-{copyIndex:000} suffix.
```

### 20.2 Inpaint example

```csharp
var response = await client.InpaintAsync(new InpaintRequest
{
    Image = IdeogramFile.FromPath("cat.png"),
    Mask = IdeogramFile.FromPath("mask.png"),
    Prompt = "A photo of a cat wearing a hat.",
    RenderingSpeed = IdeogramRenderingSpeed.Default
});
```

---

## 21. Implementation order for Codex

Follow this exact sequence.

### Phase 1 — Skeleton

1. Create solution and projects.
2. Add project references.
3. Add nullable and warning settings.
4. Create folder structure.
5. Add empty public models and constants.

Acceptance:

```bash
dotnet build
```

must pass.

### Phase 2 — Constants and validation

1. Implement all constants.
2. Implement `MimeTypeDetector`.
3. Implement `IdeogramFile.FromPath`.
4. Implement `ColorPalette` and `ColorPaletteMember`.
5. Implement `RequestValidator`.

Acceptance:

- Invalid prompt throws locally.
- Invalid seed throws locally.
- Invalid file path throws locally.
- Invalid transparent `FLASH` throws locally.
- Invalid resolution throws locally.

No test project; do ad hoc manual checks by temporarily running console or simple snippets.

### Phase 3 — Multipart builder

1. Implement `HttpContentBuilder`.
2. Ensure file streams are opened lazily.
3. Ensure all streams are disposed when multipart content is disposed.
4. Ensure content field names match the API docs exactly.
5. Ensure repeated file fields use the same field name.

Acceptance:

- Code compiles.
- Manual inspection confirms all field names match this plan.

### Phase 4 — HTTP client

1. Implement `IdeogramClientOptions`.
2. Implement `IdeogramApiException`.
3. Implement `IIdeogramClient`.
4. Implement `IdeogramClient`.
5. Implement `SendMultipartAsync`.
6. Implement all six API methods.
7. Implement image downloads.

Acceptance:

- Code compiles.
- All public methods exist.
- All endpoint paths match this plan.
- Non-success responses throw `IdeogramApiException`.
- API key header is present on Ideogram API requests as a per-request header only.
- `Api-Key` is never configured in `HttpClient.DefaultRequestHeaders`.
- Image download requests are sent through the dedicated headerless download path and never contain `Api-Key`, `Authorization`, or other API credential headers.
- `RequestTimeout` is enforced with linked operation cancellation or an equivalent per-request timeout mechanism.
- `DownloadTimeout` is enforced across the complete download lifecycle with linked operation cancellation or an equivalent per-download timeout mechanism.
- Download extension tokens have no leading dot and generated filenames never contain malformed double-dot extensions.
- `DownloadImageAsync` fails rather than overwrites when an explicit target file already exists.
- `DownloadImagesAsync` resolves generated-name collisions with `_copy-{copyIndex:000}` and returns the actual saved paths.

### Phase 5 — Console app

1. Implement API key resolution.
2. Implement interactive menu.
3. Implement per-method prompts.
4. Implement output directory creation.
5. Implement response JSON saving.
6. Implement optional image downloading.
7. Implement minimal command-line parser.

Acceptance:

- Running the console without args opens menu.
- Running with `--help` or invalid args prints supported modes.
- User can manually run all six methods.
- API key is never printed.

### Phase 6 — README and final build

1. Add README.
2. Run:

```bash
dotnet format --verify-no-changes
dotnet build
```

`dotnet format` may require the SDK workload/tooling present in the environment. If unavailable, do not add packages to make it work; run `dotnet build` only.

Final acceptance:

```bash
dotnet build
```

passes with zero warnings and zero errors.

---

## 22. Manual smoke checklist

Use a real Ideogram API key for these manual checks.

Environment setup:

```bash
export IDEOGRAM_API_KEY="your-api-key"
```

Windows PowerShell:

```powershell
$env:IDEOGRAM_API_KEY = "your-api-key"
```

### 22.1 Generate

```bash
dotnet run --project samples/Ideogram.Client.Console -- generate \
  --prompt "A photo of a cat sleeping on a couch." \
  --rendering-speed TURBO \
  --download true
```

Expected:

- HTTP 200
- `data` contains at least one image
- image downloads to output directory

### 22.2 Generate transparent

```bash
dotnet run --project samples/Ideogram.Client.Console -- transparent \
  --prompt "A minimal vector logo for Ideogram Coffee" \
  --rendering-speed TURBO \
  --upscale-factor X1 \
  --download true
```

Expected:

- HTTP 200
- output image has transparent-background generation result
- no `FLASH` rendering speed used

### 22.3 Inpaint

```bash
dotnet run --project samples/Ideogram.Client.Console -- inpaint \
  --image ./manual-assets/image.png \
  --mask ./manual-assets/mask.png \
  --prompt "A photo of a cat wearing a hat." \
  --rendering-speed DEFAULT \
  --download true
```

Expected:

- HTTP 200
- output image generated
- if mask dimensions are invalid, API returns a handled error

### 22.4 Remix

```bash
dotnet run --project samples/Ideogram.Client.Console -- remix \
  --image ./manual-assets/image.png \
  --prompt "A photo of a dog sleeping on a couch" \
  --image-weight 50 \
  --rendering-speed TURBO \
  --download true
```

Expected:

- HTTP 200
- output image generated

### 22.5 Reframe

```bash
dotnet run --project samples/Ideogram.Client.Console -- reframe \
  --image ./manual-assets/square.png \
  --resolution 1312x736 \
  --download true
```

Expected:

- HTTP 200
- output image generated at requested supported resolution

### 22.6 Replace background

```bash
dotnet run --project samples/Ideogram.Client.Console -- replace-background \
  --image ./manual-assets/person.png \
  --prompt "Add a forest in the background" \
  --rendering-speed DEFAULT \
  --download true
```

Expected:

- HTTP 200
- output image generated

---

## 23. Edge cases to handle

1. Missing API key.
2. Expired or invalid API key.
3. 400 validation error.
4. 401 unauthorized.
5. 403 forbidden.
6. 422 validation/unprocessable entity.
7. 429 rate limit.
8. 5xx server error.
9. Malformed JSON response.
10. Empty `data`.
11. Image result without URL.
12. Expired image URL during download.
13. Output file already exists:
    - `DownloadImageAsync` with an explicit `outputPath` throws `IOException` and never overwrites.
    - `DownloadImagesAsync` creates a deterministic `_copy-{copyIndex:000}` filename and never overwrites.
14. Invalid output directory.
15. User cancels with Ctrl+C.
16. File path with spaces.
17. Quoted file path.
18. Multiple reference images.
19. Character masks count mismatch.
20. Transparent request accidentally using `FLASH`.
21. Generate or remix request setting both `resolution` and `aspect_ratio`.
22. Invalid style code length.
23. Invalid color hex.
24. Color palette specifying both name and members.
25. Download response has no file extension in URL.

---

## 24. Security and privacy notes

1. Never commit API keys.
2. Never print API keys.
3. Redact API key in any caught exception text.
4. Do not include request headers in console output.
5. Never configure `Api-Key` in `HttpClient.DefaultRequestHeaders`.
6. Add `Api-Key` only to Ideogram API request messages.
7. Never send `Api-Key`, `Authorization`, `X-Api-Key`, `X-API-Key`, `X-Ideogram-Api-Key`, or other API credential headers on image download requests.
8. Use a dedicated headerless download HTTP path so signed-image downloads cannot receive API credentials.
9. Validate download URL scheme is HTTP or HTTPS.
10. Do not follow user-provided `file://` or other local URLs.
11. Save output files only under the selected output directory.
12. For downloaded file names, generate local safe names instead of trusting remote URL names.
13. Never overwrite an existing output file.
14. Do not add telemetry.
15. Do not add analytics.

---

## 25. Definition of done

The implementation is complete when:

1. `dotnet build` succeeds.
2. The library targets `net10.0`.
3. The console app targets `net10.0`.
4. There are no third-party package references.
5. `IdeogramClient` exposes all six required API methods.
6. Each API method sends `multipart/form-data`.
7. Each API request sends `Api-Key` as a per-request header, not as an `HttpClient.DefaultRequestHeaders` value.
8. Download requests never send `Api-Key`, `Authorization`, `X-Api-Key`, `X-API-Key`, `X-Ideogram-Api-Key`, or other API credential headers.
9. API and download transports are isolated so signed-image downloads cannot receive API credentials.
10. `RequestTimeout` is enforced for every API request with operation-level cancellation.
11. `DownloadTimeout` is enforced for every image download with operation-level cancellation.
12. Downloaded filenames never contain malformed double-dot extensions such as `..png`.
13. Existing output files are never overwritten.
14. `DownloadImageAsync` throws `IOException` for an explicit existing `outputPath`.
15. `DownloadImagesAsync` resolves collisions with the documented `_copy-{copyIndex:000}` suffix.
16. All request classes are public.
17. All response classes are public.
18. Request validation catches obvious local errors.
19. Console app can execute all six methods interactively.
20. Console app can execute all six methods through minimal command-line mode.
21. Console app can download returned images.
22. Console app saves pretty JSON response files.
23. API key is not printed or stored.
24. No unit test project exists.
25. No integration test project exists.
26. README documents usage.
27. `docs/PLAN.md` remains in the repository.
