# Ideogram AI API REST Client for .NET

.NET 8, .NET 9, and .NET 10 client library and companion sample apps for Ideogram AI API v3 and v4. The client talks to the API with `HttpClient`, `multipart/form-data`, and JSON where required, and also includes optional DI registration support based on `IHttpClientFactory`.

## Supported endpoints

### V4

- `POST /v1/ideogram-v4/generate` via `GenerateFromTextAsync`
- `POST /v1/ideogram-v4/generate` via `GenerateFromJsonAsync`
- `POST /v1/ideogram-v4/remix`
- `POST /v1/ideogram-v4/magic-prompt`
- `POST /v1/ideogram-v4/describe`

### V3

- `POST /v1/ideogram-v3/generate`
- `POST /v1/ideogram-v3/generate-transparent`
- `POST /v1/ideogram-v3/inpaint`
- `POST /v1/ideogram-v3/remix`
- `POST /v1/ideogram-v3/reframe`
- `POST /v1/ideogram-v3/replace-background`

## Build

```bash
dotnet restore Ideogram.slnx
dotnet build Ideogram.slnx
```

## API key setup

Set `IDEOGRAM_API_KEY` before using the library or console app.

```bash
export IDEOGRAM_API_KEY="your-api-key"
```

PowerShell:

```powershell
$env:IDEOGRAM_API_KEY = "your-api-key"
```

The console app resolves the API key in this order:

1. `--api-key <key>`
2. `IDEOGRAM_API_KEY`

If none of those sources provides a key, the console app exits with an error.



## Minimal library usage

### V3

```csharp
using A2G.Ideogram.Client;
using A2G.Ideogram.Client.Constants;
using A2G.Ideogram.Client.Models;

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
```

### V4 text prompt

```csharp
using A2G.Ideogram.Client;
using A2G.Ideogram.Client.Constants;
using A2G.Ideogram.Client.V4.Models;

var client = new IdeogramV4Client(new IdeogramClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("IDEOGRAM_API_KEY")
        ?? throw new InvalidOperationException("IDEOGRAM_API_KEY is not set.")
});

var response = await client.GenerateFromTextAsync(new GenerateFromTextRequest
{
    TextPrompt = "A clean product hero shot of a ceramic coffee mug.",
    RenderingSpeed = IdeogramRenderingSpeed.Default
});
```

### V4 structured prompt

```csharp
using A2G.Ideogram.Client.V4.Models;

var response = await client.GenerateFromJsonAsync(new GenerateFromJsonRequest
{
    JsonPrompt = new JsonPrompt
    {
        HighLevelDescription = "A modern ceramic coffee mug on a studio pedestal."
    }
});
```

## Dependency injection

The library can be registered with `IHttpClientFactory`:

```csharp
using A2G.Ideogram.Client;

builder.Services.AddIdeogramClient(new IdeogramClientOptions
{
    ApiKey = builder.Configuration["Ideogram:ApiKey"]
        ?? throw new InvalidOperationException("Ideogram:ApiKey is not configured.")
});
```

You can inject either `IIdeogramClient` or `IdeogramClient` into your services. The registration creates isolated named `HttpClient` instances for API and download traffic and keeps the existing credential-safety split intact.

For V4:

```csharp
builder.Services.AddIdeogramV4Client(new IdeogramClientOptions
{
    ApiKey = builder.Configuration["Ideogram:ApiKey"]
        ?? throw new InvalidOperationException("Ideogram:ApiKey is not configured.")
});
```

You can inject either `IIdeogramV4Client` or `IdeogramV4Client`.

Inpaint example:

```csharp
var response = await client.InpaintAsync(new InpaintRequest
{
    Image = IdeogramFile.FromPath("cat.png"),
    Mask = IdeogramFile.FromPath("mask.png"),
    Prompt = "A photo of a cat wearing a hat.",
    RenderingSpeed = IdeogramRenderingSpeed.Default
});
```

## Console usage

Interactive mode:

```bash
dotnet run --project samples/Ideogram.Client.Console
```

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
```

## Notes

- Ideogram image URLs expire, so download them immediately when needed.
- All v3 methods use `multipart/form-data`.
- V4 `magic-prompt` uses JSON; V4 `generate`, `remix`, and `describe` use `multipart/form-data`.
- The transparent endpoint does not support `FLASH`.
