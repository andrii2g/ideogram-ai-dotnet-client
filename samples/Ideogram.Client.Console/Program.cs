using System.Globalization;
using Ideogram.Client;
using Ideogram.Client.Constants;
using Ideogram.Client.Models;

namespace Ideogram.Client.ConsoleApp;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var parsedArgs = SimpleArgsParser.Parse(args, out var parseError);

        if (parsedArgs?.ShowHelp == true)
        {
            OutputWriter.PrintHelp();
            return 0;
        }

        if (parseError is not null)
        {
            System.Console.WriteLine(parseError);
            System.Console.WriteLine();
            OutputWriter.PrintHelp();
        }

        var outputDirectory = OutputWriter.CreateOutputDirectory();
        var apiKey = ResolveApiKey(parsedArgs);

        using var client = new IdeogramClient(new IdeogramClientOptions
        {
            ApiKey = apiKey
        });

        if (parsedArgs?.CommandName is not null)
        {
            try
            {
                await RunCommandLineAsync(client, parsedArgs, outputDirectory).ConfigureAwait(false);
                return 0;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Command-line execution failed: {ex.Message}");
                System.Console.WriteLine();
                OutputWriter.PrintHelp();
                System.Console.WriteLine("Falling back to interactive menu.");
                System.Console.WriteLine();
            }
        }

        await RunInteractiveMenuAsync(client, outputDirectory).ConfigureAwait(false);
        return 0;
    }

    private static string ResolveApiKey(ManualRunOptions? parsedArgs)
    {
        var apiKey = parsedArgs?.ApiKey ?? Environment.GetEnvironmentVariable("IDEOGRAM_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }

        while (true)
        {
            var secret = ConsolePrompts.ReadSecret("Enter Ideogram API key: ");
            if (!string.IsNullOrWhiteSpace(secret))
            {
                return secret;
            }

            System.Console.WriteLine("API key is required.");
        }
    }

    private static async Task RunInteractiveMenuAsync(IdeogramClient client, string outputDirectory)
    {
        while (true)
        {
            System.Console.WriteLine("Ideogram API v3 Manual Console");
            System.Console.WriteLine();
            System.Console.WriteLine("API key: loaded");
            System.Console.WriteLine($"Output directory: ./{outputDirectory.Replace('\\', '/')}");
            System.Console.WriteLine();
            System.Console.WriteLine("Select method:");
            System.Console.WriteLine("  1) Generate");
            System.Console.WriteLine("  2) Generate transparent");
            System.Console.WriteLine("  3) Inpaint");
            System.Console.WriteLine("  4) Remix");
            System.Console.WriteLine("  5) Reframe");
            System.Console.WriteLine("  6) Replace background");
            System.Console.WriteLine("  7) Download image by URL");
            System.Console.WriteLine("  0) Exit");
            System.Console.Write("> ");

            var selection = System.Console.ReadLine()?.Trim();
            System.Console.WriteLine();

            switch (selection)
            {
                case "0":
                    return;
                case "1":
                    await ExecuteGenerateInteractiveAsync(client, outputDirectory).ConfigureAwait(false);
                    break;
                case "2":
                    await ExecuteGenerateTransparentInteractiveAsync(client, outputDirectory).ConfigureAwait(false);
                    break;
                case "3":
                    System.Console.WriteLine("Mask must be the same dimensions as the image. Per Ideogram docs, black mask regions indicate the regions to edit.");
                    await ExecuteInpaintInteractiveAsync(client, outputDirectory).ConfigureAwait(false);
                    break;
                case "4":
                    await ExecuteRemixInteractiveAsync(client, outputDirectory).ConfigureAwait(false);
                    break;
                case "5":
                    System.Console.WriteLine("Common v3 resolutions:");
                    System.Console.WriteLine("  1024x1024");
                    System.Console.WriteLine("  1312x736");
                    System.Console.WriteLine("  736x1312");
                    System.Console.WriteLine("  1280x800");
                    System.Console.WriteLine("  800x1280");
                    System.Console.WriteLine("  1536x512");
                    System.Console.WriteLine("  512x1536");
                    await ExecuteReframeInteractiveAsync(client, outputDirectory).ConfigureAwait(false);
                    break;
                case "6":
                    await ExecuteReplaceBackgroundInteractiveAsync(client, outputDirectory).ConfigureAwait(false);
                    break;
                case "7":
                    await DownloadSingleImageInteractiveAsync(client, outputDirectory).ConfigureAwait(false);
                    break;
                default:
                    System.Console.WriteLine("Unknown selection.");
                    break;
            }

            System.Console.WriteLine();
        }
    }

    private static async Task RunCommandLineAsync(IdeogramClient client, ManualRunOptions parsedArgs, string outputDirectory)
    {
        switch (parsedArgs.CommandName!.ToLowerInvariant())
        {
            case "generate":
                await ExecuteRequestAsync(
                    client,
                    outputDirectory,
                    "generate",
                    BuildGenerateFromArgs(parsedArgs),
                    parsedArgs.Download ?? false,
                    static (c, request) => c.GenerateAsync(request)).ConfigureAwait(false);
                break;
            case "transparent":
                await ExecuteRequestAsync(
                    client,
                    outputDirectory,
                    "transparent",
                    BuildGenerateTransparentFromArgs(parsedArgs),
                    parsedArgs.Download ?? false,
                    static (c, request) => c.GenerateTransparentAsync(request)).ConfigureAwait(false);
                break;
            case "inpaint":
                await ExecuteRequestAsync(
                    client,
                    outputDirectory,
                    "inpaint",
                    BuildInpaintFromArgs(parsedArgs),
                    parsedArgs.Download ?? false,
                    static (c, request) => c.InpaintAsync(request)).ConfigureAwait(false);
                break;
            case "remix":
                await ExecuteRequestAsync(
                    client,
                    outputDirectory,
                    "remix",
                    BuildRemixFromArgs(parsedArgs),
                    parsedArgs.Download ?? false,
                    static (c, request) => c.RemixAsync(request)).ConfigureAwait(false);
                break;
            case "reframe":
                await ExecuteRequestAsync(
                    client,
                    outputDirectory,
                    "reframe",
                    BuildReframeFromArgs(parsedArgs),
                    parsedArgs.Download ?? false,
                    static (c, request) => c.ReframeAsync(request)).ConfigureAwait(false);
                break;
            case "replace-background":
                await ExecuteRequestAsync(
                    client,
                    outputDirectory,
                    "replace-background",
                    BuildReplaceBackgroundFromArgs(parsedArgs),
                    parsedArgs.Download ?? false,
                    static (c, request) => c.ReplaceBackgroundAsync(request)).ConfigureAwait(false);
                break;
            case "download":
                var url = RequireArg(parsedArgs, "url");
                var outputPath = SimpleArgsParser.GetValue(parsedArgs, "output") ??
                                 Path.Combine(outputDirectory, $"downloaded.{DetermineExtensionToken(url)}");
                var savedPath = await client.DownloadImageAsync(url, outputPath).ConfigureAwait(false);
                OutputWriter.PrintSavedImagePaths([savedPath]);
                break;
            default:
                throw new ArgumentException($"Unknown command '{parsedArgs.CommandName}'.");
        }
    }

    private static async Task ExecuteGenerateInteractiveAsync(IdeogramClient client, string outputDirectory)
    {
        var request = new GenerateRequest
        {
            Prompt = ConsolePrompts.RequiredString("Prompt"),
            Seed = ConsolePrompts.OptionalInt("Seed", min: 0),
            Resolution = ConsolePrompts.OptionalString("Resolution"),
            AspectRatio = ConsolePrompts.OptionalString("Aspect ratio"),
            RenderingSpeed = ConsolePrompts.OptionalString("Rendering speed", IdeogramRenderingSpeed.Turbo),
            MagicPrompt = ConsolePrompts.OptionalString("Magic prompt"),
            NegativePrompt = ConsolePrompts.OptionalString("Negative prompt"),
            NumImages = ConsolePrompts.OptionalInt("Num images", 1, 1, 8),
            StyleType = ConsolePrompts.OptionalString("Style type", IdeogramStyleTypes.General),
            StylePreset = ConsolePrompts.OptionalString("Style preset"),
            CustomModelUri = ConsolePrompts.OptionalString("Custom model URI"),
            StyleCodes = ConsolePrompts.OptionalStringList("Style codes"),
            ColorPalette = PromptColorPalette(),
            StyleReferenceImages = ConsolePrompts.OptionalImageFiles("Style reference images"),
            CharacterReferenceImages = ConsolePrompts.OptionalImageFiles("Character reference images"),
            CharacterReferenceImageMasks = ConsolePrompts.OptionalImageFiles("Character reference image masks")
        };

        await ExecuteRequestAsync(
            client,
            outputDirectory,
            "generate",
            request,
            shouldDownload: false,
            static (c, model) => c.GenerateAsync(model),
            defaultDownloadPrompt: true).ConfigureAwait(false);
    }

    private static async Task ExecuteGenerateTransparentInteractiveAsync(IdeogramClient client, string outputDirectory)
    {
        var request = new GenerateTransparentRequest
        {
            Prompt = ConsolePrompts.RequiredString("Prompt"),
            Seed = ConsolePrompts.OptionalInt("Seed", min: 0),
            UpscaleFactor = ConsolePrompts.OptionalString("Upscale factor", IdeogramUpscaleFactors.X1),
            AspectRatio = ConsolePrompts.OptionalString("Aspect ratio"),
            RenderingSpeed = ConsolePrompts.OptionalString("Rendering speed", IdeogramRenderingSpeed.Turbo),
            MagicPrompt = ConsolePrompts.OptionalString("Magic prompt"),
            NegativePrompt = ConsolePrompts.OptionalString("Negative prompt"),
            NumImages = ConsolePrompts.OptionalInt("Num images", 1, 1, 8)
        };

        await ExecuteRequestAsync(
            client,
            outputDirectory,
            "transparent",
            request,
            shouldDownload: false,
            static (c, model) => c.GenerateTransparentAsync(model),
            defaultDownloadPrompt: true).ConfigureAwait(false);
    }

    private static async Task ExecuteInpaintInteractiveAsync(IdeogramClient client, string outputDirectory)
    {
        var request = new InpaintRequest
        {
            Image = ConsolePrompts.RequiredImageFile("Image path"),
            Mask = ConsolePrompts.RequiredImageFile("Mask path"),
            Prompt = ConsolePrompts.RequiredString("Prompt"),
            MagicPrompt = ConsolePrompts.OptionalString("Magic prompt"),
            NumImages = ConsolePrompts.OptionalInt("Num images", 1, 1, 8),
            Seed = ConsolePrompts.OptionalInt("Seed", min: 0),
            RenderingSpeed = ConsolePrompts.OptionalString("Rendering speed", IdeogramRenderingSpeed.Default),
            StyleType = ConsolePrompts.OptionalString("Style type"),
            StylePreset = ConsolePrompts.OptionalString("Style preset"),
            ColorPalette = PromptColorPalette(),
            StyleCodes = ConsolePrompts.OptionalStringList("Style codes"),
            StyleReferenceImages = ConsolePrompts.OptionalImageFiles("Style reference images"),
            CharacterReferenceImages = ConsolePrompts.OptionalImageFiles("Character reference images"),
            CharacterReferenceImageMasks = ConsolePrompts.OptionalImageFiles("Character reference image masks")
        };

        await ExecuteRequestAsync(
            client,
            outputDirectory,
            "inpaint",
            request,
            shouldDownload: false,
            static (c, model) => c.InpaintAsync(model),
            defaultDownloadPrompt: true).ConfigureAwait(false);
    }

    private static async Task ExecuteRemixInteractiveAsync(IdeogramClient client, string outputDirectory)
    {
        var request = new RemixRequest
        {
            Image = ConsolePrompts.RequiredImageFile("Image path"),
            Prompt = ConsolePrompts.RequiredString("Prompt"),
            ImageWeight = ConsolePrompts.OptionalInt("Image weight", 50, 1, 100),
            Seed = ConsolePrompts.OptionalInt("Seed", min: 0),
            Resolution = ConsolePrompts.OptionalString("Resolution"),
            AspectRatio = ConsolePrompts.OptionalString("Aspect ratio"),
            RenderingSpeed = ConsolePrompts.OptionalString("Rendering speed", IdeogramRenderingSpeed.Turbo),
            MagicPrompt = ConsolePrompts.OptionalString("Magic prompt"),
            NegativePrompt = ConsolePrompts.OptionalString("Negative prompt"),
            NumImages = ConsolePrompts.OptionalInt("Num images", 1, 1, 8),
            ColorPalette = PromptColorPalette(),
            StyleCodes = ConsolePrompts.OptionalStringList("Style codes"),
            StyleType = ConsolePrompts.OptionalString("Style type", IdeogramStyleTypes.General),
            StylePreset = ConsolePrompts.OptionalString("Style preset"),
            StyleReferenceImages = ConsolePrompts.OptionalImageFiles("Style reference images"),
            CharacterReferenceImages = ConsolePrompts.OptionalImageFiles("Character reference images"),
            CharacterReferenceImageMasks = ConsolePrompts.OptionalImageFiles("Character reference image masks")
        };

        await ExecuteRequestAsync(
            client,
            outputDirectory,
            "remix",
            request,
            shouldDownload: false,
            static (c, model) => c.RemixAsync(model),
            defaultDownloadPrompt: true).ConfigureAwait(false);
    }

    private static async Task ExecuteReframeInteractiveAsync(IdeogramClient client, string outputDirectory)
    {
        var request = new ReframeRequest
        {
            Image = ConsolePrompts.RequiredImageFile("Image path"),
            Resolution = ConsolePrompts.RequiredString("Resolution"),
            NumImages = ConsolePrompts.OptionalInt("Num images", 1, 1, 8),
            Seed = ConsolePrompts.OptionalInt("Seed", min: 0),
            RenderingSpeed = ConsolePrompts.OptionalString("Rendering speed", IdeogramRenderingSpeed.Default),
            StylePreset = ConsolePrompts.OptionalString("Style preset"),
            ColorPalette = PromptColorPalette(),
            StyleCodes = ConsolePrompts.OptionalStringList("Style codes"),
            StyleReferenceImages = ConsolePrompts.OptionalImageFiles("Style reference images")
        };

        await ExecuteRequestAsync(
            client,
            outputDirectory,
            "reframe",
            request,
            shouldDownload: false,
            static (c, model) => c.ReframeAsync(model),
            defaultDownloadPrompt: true).ConfigureAwait(false);
    }

    private static async Task ExecuteReplaceBackgroundInteractiveAsync(IdeogramClient client, string outputDirectory)
    {
        var request = new ReplaceBackgroundRequest
        {
            Image = ConsolePrompts.RequiredImageFile("Image path"),
            Prompt = ConsolePrompts.RequiredString("Prompt"),
            MagicPrompt = ConsolePrompts.OptionalString("Magic prompt"),
            NumImages = ConsolePrompts.OptionalInt("Num images", 1, 1, 8),
            Seed = ConsolePrompts.OptionalInt("Seed", min: 0),
            RenderingSpeed = ConsolePrompts.OptionalString("Rendering speed", IdeogramRenderingSpeed.Default),
            StylePreset = ConsolePrompts.OptionalString("Style preset"),
            ColorPalette = PromptColorPalette(),
            StyleCodes = ConsolePrompts.OptionalStringList("Style codes"),
            StyleReferenceImages = ConsolePrompts.OptionalImageFiles("Style reference images")
        };

        await ExecuteRequestAsync(
            client,
            outputDirectory,
            "replace-background",
            request,
            shouldDownload: false,
            static (c, model) => c.ReplaceBackgroundAsync(model),
            defaultDownloadPrompt: true).ConfigureAwait(false);
    }

    private static async Task DownloadSingleImageInteractiveAsync(IdeogramClient client, string outputDirectory)
    {
        var imageUrl = ConsolePrompts.RequiredString("Image URL");
        var outputPath = ConsolePrompts.OptionalPath("Output path") ??
                         Path.Combine(outputDirectory, $"downloaded.{DetermineExtensionToken(imageUrl)}");

        try
        {
            var savedPath = await client.DownloadImageAsync(imageUrl, outputPath).ConfigureAwait(false);
            OutputWriter.PrintSavedImagePaths([savedPath]);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }
    }

    private static async Task ExecuteRequestAsync<TRequest>(
        IdeogramClient client,
        string outputDirectory,
        string methodName,
        TRequest request,
        bool shouldDownload,
        Func<IdeogramClient, TRequest, Task<IdeogramResponse>> sendAsync,
        bool defaultDownloadPrompt)
    {
        try
        {
            var response = await sendAsync(client, request).ConfigureAwait(false);
            OutputWriter.PrintResponseSummary(response);
            var responsePath = OutputWriter.SaveResponseJson(outputDirectory, methodName, response);
            OutputWriter.PrintSavedResponsePath(responsePath);

            if (shouldDownload || ConsolePrompts.Confirm("Download returned images now?", defaultDownloadPrompt))
            {
                var savedImages = await client.DownloadImagesAsync(response, outputDirectory, methodName).ConfigureAwait(false);
                OutputWriter.PrintSavedImagePaths(savedImages);
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }
    }

    private static GenerateRequest BuildGenerateFromArgs(ManualRunOptions options)
    {
        return new GenerateRequest
        {
            Prompt = RequireArg(options, "prompt"),
            Seed = ParseOptionalInt(SimpleArgsParser.GetValue(options, "seed")),
            Resolution = SimpleArgsParser.GetValue(options, "resolution"),
            AspectRatio = SimpleArgsParser.GetValue(options, "aspect-ratio"),
            RenderingSpeed = SimpleArgsParser.GetValue(options, "rendering-speed"),
            MagicPrompt = SimpleArgsParser.GetValue(options, "magic-prompt"),
            NegativePrompt = SimpleArgsParser.GetValue(options, "negative-prompt"),
            NumImages = ParseOptionalInt(SimpleArgsParser.GetValue(options, "num-images")),
            StyleType = SimpleArgsParser.GetValue(options, "style-type"),
            StylePreset = SimpleArgsParser.GetValue(options, "style-preset"),
            CustomModelUri = SimpleArgsParser.GetValue(options, "custom-model-uri"),
            StyleCodes = ParseStringList(SimpleArgsParser.GetValue(options, "style-codes")),
            ColorPalette = ParseColorPaletteFromArgs(options),
            StyleReferenceImages = ParseImageFiles(SimpleArgsParser.GetValue(options, "style-reference-images")),
            CharacterReferenceImages = ParseImageFiles(SimpleArgsParser.GetValue(options, "character-reference-images")),
            CharacterReferenceImageMasks = ParseImageFiles(SimpleArgsParser.GetValue(options, "character-reference-image-masks"))
        };
    }

    private static GenerateTransparentRequest BuildGenerateTransparentFromArgs(ManualRunOptions options)
    {
        return new GenerateTransparentRequest
        {
            Prompt = RequireArg(options, "prompt"),
            Seed = ParseOptionalInt(SimpleArgsParser.GetValue(options, "seed")),
            UpscaleFactor = SimpleArgsParser.GetValue(options, "upscale-factor"),
            AspectRatio = SimpleArgsParser.GetValue(options, "aspect-ratio"),
            RenderingSpeed = SimpleArgsParser.GetValue(options, "rendering-speed"),
            MagicPrompt = SimpleArgsParser.GetValue(options, "magic-prompt"),
            NegativePrompt = SimpleArgsParser.GetValue(options, "negative-prompt"),
            NumImages = ParseOptionalInt(SimpleArgsParser.GetValue(options, "num-images"))
        };
    }

    private static InpaintRequest BuildInpaintFromArgs(ManualRunOptions options)
    {
        return new InpaintRequest
        {
            Image = IdeogramFile.FromPath(RequireArg(options, "image")),
            Mask = IdeogramFile.FromPath(RequireArg(options, "mask")),
            Prompt = RequireArg(options, "prompt"),
            MagicPrompt = SimpleArgsParser.GetValue(options, "magic-prompt"),
            NumImages = ParseOptionalInt(SimpleArgsParser.GetValue(options, "num-images")),
            Seed = ParseOptionalInt(SimpleArgsParser.GetValue(options, "seed")),
            RenderingSpeed = SimpleArgsParser.GetValue(options, "rendering-speed"),
            StyleType = SimpleArgsParser.GetValue(options, "style-type"),
            StylePreset = SimpleArgsParser.GetValue(options, "style-preset"),
            ColorPalette = ParseColorPaletteFromArgs(options),
            StyleCodes = ParseStringList(SimpleArgsParser.GetValue(options, "style-codes")),
            StyleReferenceImages = ParseImageFiles(SimpleArgsParser.GetValue(options, "style-reference-images")),
            CharacterReferenceImages = ParseImageFiles(SimpleArgsParser.GetValue(options, "character-reference-images")),
            CharacterReferenceImageMasks = ParseImageFiles(SimpleArgsParser.GetValue(options, "character-reference-image-masks"))
        };
    }

    private static RemixRequest BuildRemixFromArgs(ManualRunOptions options)
    {
        return new RemixRequest
        {
            Image = IdeogramFile.FromPath(RequireArg(options, "image")),
            Prompt = RequireArg(options, "prompt"),
            ImageWeight = ParseOptionalInt(SimpleArgsParser.GetValue(options, "image-weight")),
            Seed = ParseOptionalInt(SimpleArgsParser.GetValue(options, "seed")),
            Resolution = SimpleArgsParser.GetValue(options, "resolution"),
            AspectRatio = SimpleArgsParser.GetValue(options, "aspect-ratio"),
            RenderingSpeed = SimpleArgsParser.GetValue(options, "rendering-speed"),
            MagicPrompt = SimpleArgsParser.GetValue(options, "magic-prompt"),
            NegativePrompt = SimpleArgsParser.GetValue(options, "negative-prompt"),
            NumImages = ParseOptionalInt(SimpleArgsParser.GetValue(options, "num-images")),
            ColorPalette = ParseColorPaletteFromArgs(options),
            StyleCodes = ParseStringList(SimpleArgsParser.GetValue(options, "style-codes")),
            StyleType = SimpleArgsParser.GetValue(options, "style-type"),
            StylePreset = SimpleArgsParser.GetValue(options, "style-preset"),
            StyleReferenceImages = ParseImageFiles(SimpleArgsParser.GetValue(options, "style-reference-images")),
            CharacterReferenceImages = ParseImageFiles(SimpleArgsParser.GetValue(options, "character-reference-images")),
            CharacterReferenceImageMasks = ParseImageFiles(SimpleArgsParser.GetValue(options, "character-reference-image-masks"))
        };
    }

    private static ReframeRequest BuildReframeFromArgs(ManualRunOptions options)
    {
        return new ReframeRequest
        {
            Image = IdeogramFile.FromPath(RequireArg(options, "image")),
            Resolution = RequireArg(options, "resolution"),
            NumImages = ParseOptionalInt(SimpleArgsParser.GetValue(options, "num-images")),
            Seed = ParseOptionalInt(SimpleArgsParser.GetValue(options, "seed")),
            RenderingSpeed = SimpleArgsParser.GetValue(options, "rendering-speed"),
            StylePreset = SimpleArgsParser.GetValue(options, "style-preset"),
            ColorPalette = ParseColorPaletteFromArgs(options),
            StyleCodes = ParseStringList(SimpleArgsParser.GetValue(options, "style-codes")),
            StyleReferenceImages = ParseImageFiles(SimpleArgsParser.GetValue(options, "style-reference-images"))
        };
    }

    private static ReplaceBackgroundRequest BuildReplaceBackgroundFromArgs(ManualRunOptions options)
    {
        return new ReplaceBackgroundRequest
        {
            Image = IdeogramFile.FromPath(RequireArg(options, "image")),
            Prompt = RequireArg(options, "prompt"),
            MagicPrompt = SimpleArgsParser.GetValue(options, "magic-prompt"),
            NumImages = ParseOptionalInt(SimpleArgsParser.GetValue(options, "num-images")),
            Seed = ParseOptionalInt(SimpleArgsParser.GetValue(options, "seed")),
            RenderingSpeed = SimpleArgsParser.GetValue(options, "rendering-speed"),
            StylePreset = SimpleArgsParser.GetValue(options, "style-preset"),
            ColorPalette = ParseColorPaletteFromArgs(options),
            StyleCodes = ParseStringList(SimpleArgsParser.GetValue(options, "style-codes")),
            StyleReferenceImages = ParseImageFiles(SimpleArgsParser.GetValue(options, "style-reference-images"))
        };
    }

    private static ColorPalette? PromptColorPalette()
    {
        var preset = ConsolePrompts.OptionalString("Color palette preset");
        if (!string.IsNullOrWhiteSpace(preset))
        {
            return ColorPalette.FromPreset(preset);
        }

        var members = ConsolePrompts.OptionalStringList("Custom palette members as #RRGGBB[:weight]");
        if (members is null)
        {
            return null;
        }

        return ColorPalette.FromMembers(members.Select(ParseColorPaletteMember).ToArray());
    }

    private static ColorPalette? ParseColorPaletteFromArgs(ManualRunOptions options)
    {
        var preset = SimpleArgsParser.GetValue(options, "color-palette");
        if (!string.IsNullOrWhiteSpace(preset))
        {
            return ColorPalette.FromPreset(preset);
        }

        var members = ParseStringList(SimpleArgsParser.GetValue(options, "color-palette-members"));
        if (members is null)
        {
            return null;
        }

        return ColorPalette.FromMembers(members.Select(ParseColorPaletteMember).ToArray());
    }

    private static ColorPaletteMember ParseColorPaletteMember(string value)
    {
        var separatorIndex = value.LastIndexOf(':');
        if (separatorIndex > 0)
        {
            return new ColorPaletteMember
            {
                ColorHex = value[..separatorIndex].Trim(),
                ColorWeight = double.Parse(value[(separatorIndex + 1)..].Trim(), CultureInfo.InvariantCulture)
            };
        }

        return new ColorPaletteMember
        {
            ColorHex = value,
            ColorWeight = null
        };
    }

    private static IReadOnlyList<IdeogramFile>? ParseImageFiles(string? input)
    {
        var paths = ParseStringList(input);
        return paths?.Select(IdeogramFile.FromPath).ToArray();
    }

    private static IReadOnlyList<string>? ParseStringList(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var values = input
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        return values.Length == 0 ? null : values;
    }

    private static int? ParseOptionalInt(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? null
            : int.Parse(input, CultureInfo.InvariantCulture);
    }

    private static string RequireArg(ManualRunOptions options, string key)
    {
        return SimpleArgsParser.GetValue(options, key)
            ?? throw new ArgumentException($"Missing required option '--{key}'.");
    }

    private static string DetermineExtensionToken(string imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            return "png";
        }

        return Path.GetExtension(uri.AbsolutePath).ToLowerInvariant() switch
        {
            ".jpg" => "jpg",
            ".jpeg" => "jpeg",
            ".webp" => "webp",
            _ => "png"
        };
    }
}
