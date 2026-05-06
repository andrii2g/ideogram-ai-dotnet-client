# Ideogram AI API v3 REST Client for .NET 10

Dependency-free `.NET 10` client library and companion console app for Ideogram AI API v3. The implementation uses only the .NET Base Class Library and talks to the API with `HttpClient` and `multipart/form-data`.

## Supported endpoints

- `POST /v1/ideogram-v3/generate`
- `POST /v1/ideogram-v3/generate-transparent`
- `POST /v1/ideogram-v3/inpaint`
- `POST /v1/ideogram-v3/remix`
- `POST /v1/ideogram-v3/reframe`
- `POST /v1/ideogram-v3/replace-background`

## Package policy

This repository intentionally uses no third-party NuGet packages.

## Build

```bash
dotnet restore IdeogramV3DotNet.slnx
dotnet build IdeogramV3DotNet.slnx
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

```csharp
using Ideogram.Client;
using Ideogram.Client.Constants;
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
```

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
- The transparent endpoint does not support `FLASH`.
- No tests are included by request.
