using A2G.Ideogram.Client;
using A2G.Ideogram.Client.Constants;
using A2G.Ideogram.Client.Models;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.Globalization;

namespace A2G.Ideogram.Client.ConsoleApp;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var rootCommand = BuildRootCommand();
        return await rootCommand.Parse(args).InvokeAsync().ConfigureAwait(false);
    }

    private static RootCommand BuildRootCommand()
    {
        var apiKeyOption = new Option<string?>("--api-key")
        {
            Description = "Ideogram API key. Falls back to IDEOGRAM_API_KEY or User Secrets.",
            Recursive = true
        };

        var rootCommand = new RootCommand("Ideogram.Client samples")
        {
            Options =
            {
                apiKeyOption
            }
        };

        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var outputDirectory = OutputWriter.CreateOutputDirectory();

            using var client = CreateClient(parseResult.GetValue(apiKeyOption));
            await RunInteractiveMenuAsync(client, outputDirectory, cancellationToken).ConfigureAwait(false);
            return 0;
        });

        rootCommand.Subcommands.Add(CreateGenerateCommand(apiKeyOption));
        rootCommand.Subcommands.Add(CreateGenerateTransparentCommand(apiKeyOption));
        rootCommand.Subcommands.Add(CreateInpaintCommand(apiKeyOption));
        rootCommand.Subcommands.Add(CreateRemixCommand(apiKeyOption));
        rootCommand.Subcommands.Add(CreateReframeCommand(apiKeyOption));
        rootCommand.Subcommands.Add(CreateReplaceBackgroundCommand(apiKeyOption));
        rootCommand.Subcommands.Add(CreateDownloadCommand(apiKeyOption));

        return rootCommand;
    }

    private static Command CreateGenerateCommand(Option<string?> apiKeyOption)
    {
        var promptOption = CreateRequiredStringOption("--prompt", "Prompt text.");
        var seedOption = CreateIntOption("--seed", "Seed value.");
        var resolutionOption = CreateStringOption("--resolution", "Resolution such as 1024x1024.");
        var aspectRatioOption = CreateStringOption("--aspect-ratio", "Aspect ratio such as 1x1.");
        var renderingSpeedOption = CreateStringOption("--rendering-speed", "Rendering speed.");
        var magicPromptOption = CreateStringOption("--magic-prompt", "Magic prompt option.");
        var negativePromptOption = CreateStringOption("--negative-prompt", "Negative prompt.");
        var numImagesOption = CreateIntOption("--num-images", "Number of images.");
        var styleTypeOption = CreateStringOption("--style-type", "Style type.");
        var stylePresetOption = CreateStringOption("--style-preset", "Style preset.");
        var customModelUriOption = CreateStringOption("--custom-model-uri", "Custom model URI.");
        var styleCodesOption = CreateStringOption("--style-codes", "Semicolon-separated style codes.");
        var colorPaletteOption = CreateStringOption("--color-palette", "Color palette preset.");
        var colorPaletteMembersOption = CreateStringOption("--color-palette-members", "Semicolon-separated #RRGGBB[:weight] members.");
        var styleReferenceImagesOption = CreateStringOption("--style-reference-images", "Semicolon-separated style reference image paths.");
        var characterReferenceImagesOption = CreateStringOption("--character-reference-images", "Semicolon-separated character reference image paths.");
        var characterReferenceImageMasksOption = CreateStringOption("--character-reference-image-masks", "Semicolon-separated character reference mask paths.");
        var downloadOption = CreateDownloadOption();

        var command = new Command("generate", "Run the generate sample.");
        command.Options.Add(promptOption);
        command.Options.Add(seedOption);
        command.Options.Add(resolutionOption);
        command.Options.Add(aspectRatioOption);
        command.Options.Add(renderingSpeedOption);
        command.Options.Add(magicPromptOption);
        command.Options.Add(negativePromptOption);
        command.Options.Add(numImagesOption);
        command.Options.Add(styleTypeOption);
        command.Options.Add(stylePresetOption);
        command.Options.Add(customModelUriOption);
        command.Options.Add(styleCodesOption);
        command.Options.Add(colorPaletteOption);
        command.Options.Add(colorPaletteMembersOption);
        command.Options.Add(styleReferenceImagesOption);
        command.Options.Add(characterReferenceImagesOption);
        command.Options.Add(characterReferenceImageMasksOption);
        command.Options.Add(downloadOption);

        command.SetAction((parseResult, cancellationToken) =>
        {
            var options = new GenerateSampleOptions
            {
                Prompt = parseResult.GetValue(promptOption)!,
                Seed = parseResult.GetValue(seedOption),
                Resolution = parseResult.GetValue(resolutionOption),
                AspectRatio = parseResult.GetValue(aspectRatioOption),
                RenderingSpeed = parseResult.GetValue(renderingSpeedOption),
                MagicPrompt = parseResult.GetValue(magicPromptOption),
                NegativePrompt = parseResult.GetValue(negativePromptOption),
                NumImages = parseResult.GetValue(numImagesOption),
                StyleType = parseResult.GetValue(styleTypeOption),
                StylePreset = parseResult.GetValue(stylePresetOption),
                CustomModelUri = parseResult.GetValue(customModelUriOption),
                StyleCodes = ParseStringList(parseResult.GetValue(styleCodesOption)),
                ColorPalette = ParseColorPalette(parseResult.GetValue(colorPaletteOption), parseResult.GetValue(colorPaletteMembersOption)),
                StyleReferenceImages = ParseImageFiles(parseResult.GetValue(styleReferenceImagesOption)),
                CharacterReferenceImages = ParseImageFiles(parseResult.GetValue(characterReferenceImagesOption)),
                CharacterReferenceImageMasks = ParseImageFiles(parseResult.GetValue(characterReferenceImageMasksOption))
            };

            return ExecuteSampleCommandAsync(
                parseResult.GetValue(apiKeyOption),
                "generate",
                parseResult.GetValue(downloadOption),
                (client, token) => IdeogramClientSamples.GenerateAsync(client, options, token),
                cancellationToken);
        });

        return command;
    }

    private static Command CreateGenerateTransparentCommand(Option<string?> apiKeyOption)
    {
        var promptOption = CreateRequiredStringOption("--prompt", "Prompt text.");
        var seedOption = CreateIntOption("--seed", "Seed value.");
        var upscaleFactorOption = CreateStringOption("--upscale-factor", "Upscale factor.");
        var aspectRatioOption = CreateStringOption("--aspect-ratio", "Aspect ratio.");
        var renderingSpeedOption = CreateStringOption("--rendering-speed", "Rendering speed.");
        var magicPromptOption = CreateStringOption("--magic-prompt", "Magic prompt option.");
        var negativePromptOption = CreateStringOption("--negative-prompt", "Negative prompt.");
        var numImagesOption = CreateIntOption("--num-images", "Number of images.");
        var downloadOption = CreateDownloadOption();

        var command = new Command("transparent", "Run the transparent generation sample.");
        command.Options.Add(promptOption);
        command.Options.Add(seedOption);
        command.Options.Add(upscaleFactorOption);
        command.Options.Add(aspectRatioOption);
        command.Options.Add(renderingSpeedOption);
        command.Options.Add(magicPromptOption);
        command.Options.Add(negativePromptOption);
        command.Options.Add(numImagesOption);
        command.Options.Add(downloadOption);

        command.SetAction((parseResult, cancellationToken) =>
        {
            var options = new GenerateTransparentSampleOptions
            {
                Prompt = parseResult.GetValue(promptOption)!,
                Seed = parseResult.GetValue(seedOption),
                UpscaleFactor = parseResult.GetValue(upscaleFactorOption),
                AspectRatio = parseResult.GetValue(aspectRatioOption),
                RenderingSpeed = parseResult.GetValue(renderingSpeedOption),
                MagicPrompt = parseResult.GetValue(magicPromptOption),
                NegativePrompt = parseResult.GetValue(negativePromptOption),
                NumImages = parseResult.GetValue(numImagesOption)
            };

            return ExecuteSampleCommandAsync(
                parseResult.GetValue(apiKeyOption),
                "transparent",
                parseResult.GetValue(downloadOption),
                (client, token) => IdeogramClientSamples.GenerateTransparentAsync(client, options, token),
                cancellationToken);
        });

        return command;
    }

    private static Command CreateInpaintCommand(Option<string?> apiKeyOption)
    {
        var imageOption = CreateRequiredStringOption("--image", "Input image path.");
        var maskOption = CreateRequiredStringOption("--mask", "Mask image path.");
        var promptOption = CreateRequiredStringOption("--prompt", "Prompt text.");
        var magicPromptOption = CreateStringOption("--magic-prompt", "Magic prompt option.");
        var numImagesOption = CreateIntOption("--num-images", "Number of images.");
        var seedOption = CreateIntOption("--seed", "Seed value.");
        var renderingSpeedOption = CreateStringOption("--rendering-speed", "Rendering speed.");
        var styleTypeOption = CreateStringOption("--style-type", "Style type.");
        var stylePresetOption = CreateStringOption("--style-preset", "Style preset.");
        var colorPaletteOption = CreateStringOption("--color-palette", "Color palette preset.");
        var colorPaletteMembersOption = CreateStringOption("--color-palette-members", "Semicolon-separated #RRGGBB[:weight] members.");
        var styleCodesOption = CreateStringOption("--style-codes", "Semicolon-separated style codes.");
        var styleReferenceImagesOption = CreateStringOption("--style-reference-images", "Semicolon-separated style reference image paths.");
        var characterReferenceImagesOption = CreateStringOption("--character-reference-images", "Semicolon-separated character reference image paths.");
        var characterReferenceImageMasksOption = CreateStringOption("--character-reference-image-masks", "Semicolon-separated character reference mask paths.");
        var downloadOption = CreateDownloadOption();

        var command = new Command("inpaint", "Run the inpaint sample.");
        command.Options.Add(imageOption);
        command.Options.Add(maskOption);
        command.Options.Add(promptOption);
        command.Options.Add(magicPromptOption);
        command.Options.Add(numImagesOption);
        command.Options.Add(seedOption);
        command.Options.Add(renderingSpeedOption);
        command.Options.Add(styleTypeOption);
        command.Options.Add(stylePresetOption);
        command.Options.Add(colorPaletteOption);
        command.Options.Add(colorPaletteMembersOption);
        command.Options.Add(styleCodesOption);
        command.Options.Add(styleReferenceImagesOption);
        command.Options.Add(characterReferenceImagesOption);
        command.Options.Add(characterReferenceImageMasksOption);
        command.Options.Add(downloadOption);

        command.SetAction((parseResult, cancellationToken) =>
        {
            var options = new InpaintSampleOptions
            {
                Image = IdeogramFile.FromPath(parseResult.GetValue(imageOption)!),
                Mask = IdeogramFile.FromPath(parseResult.GetValue(maskOption)!),
                Prompt = parseResult.GetValue(promptOption)!,
                MagicPrompt = parseResult.GetValue(magicPromptOption),
                NumImages = parseResult.GetValue(numImagesOption),
                Seed = parseResult.GetValue(seedOption),
                RenderingSpeed = parseResult.GetValue(renderingSpeedOption),
                StyleType = parseResult.GetValue(styleTypeOption),
                StylePreset = parseResult.GetValue(stylePresetOption),
                ColorPalette = ParseColorPalette(parseResult.GetValue(colorPaletteOption), parseResult.GetValue(colorPaletteMembersOption)),
                StyleCodes = ParseStringList(parseResult.GetValue(styleCodesOption)),
                StyleReferenceImages = ParseImageFiles(parseResult.GetValue(styleReferenceImagesOption)),
                CharacterReferenceImages = ParseImageFiles(parseResult.GetValue(characterReferenceImagesOption)),
                CharacterReferenceImageMasks = ParseImageFiles(parseResult.GetValue(characterReferenceImageMasksOption))
            };

            return ExecuteSampleCommandAsync(
                parseResult.GetValue(apiKeyOption),
                "inpaint",
                parseResult.GetValue(downloadOption),
                (client, token) => IdeogramClientSamples.InpaintAsync(client, options, token),
                cancellationToken);
        });

        return command;
    }

    private static Command CreateRemixCommand(Option<string?> apiKeyOption)
    {
        var imageOption = CreateRequiredStringOption("--image", "Input image path.");
        var promptOption = CreateRequiredStringOption("--prompt", "Prompt text.");
        var imageWeightOption = CreateIntOption("--image-weight", "Image weight.");
        var seedOption = CreateIntOption("--seed", "Seed value.");
        var resolutionOption = CreateStringOption("--resolution", "Resolution.");
        var aspectRatioOption = CreateStringOption("--aspect-ratio", "Aspect ratio.");
        var renderingSpeedOption = CreateStringOption("--rendering-speed", "Rendering speed.");
        var magicPromptOption = CreateStringOption("--magic-prompt", "Magic prompt option.");
        var negativePromptOption = CreateStringOption("--negative-prompt", "Negative prompt.");
        var numImagesOption = CreateIntOption("--num-images", "Number of images.");
        var colorPaletteOption = CreateStringOption("--color-palette", "Color palette preset.");
        var colorPaletteMembersOption = CreateStringOption("--color-palette-members", "Semicolon-separated #RRGGBB[:weight] members.");
        var styleCodesOption = CreateStringOption("--style-codes", "Semicolon-separated style codes.");
        var styleTypeOption = CreateStringOption("--style-type", "Style type.");
        var stylePresetOption = CreateStringOption("--style-preset", "Style preset.");
        var styleReferenceImagesOption = CreateStringOption("--style-reference-images", "Semicolon-separated style reference image paths.");
        var characterReferenceImagesOption = CreateStringOption("--character-reference-images", "Semicolon-separated character reference image paths.");
        var characterReferenceImageMasksOption = CreateStringOption("--character-reference-image-masks", "Semicolon-separated character reference mask paths.");
        var downloadOption = CreateDownloadOption();

        var command = new Command("remix", "Run the remix sample.");
        command.Options.Add(imageOption);
        command.Options.Add(promptOption);
        command.Options.Add(imageWeightOption);
        command.Options.Add(seedOption);
        command.Options.Add(resolutionOption);
        command.Options.Add(aspectRatioOption);
        command.Options.Add(renderingSpeedOption);
        command.Options.Add(magicPromptOption);
        command.Options.Add(negativePromptOption);
        command.Options.Add(numImagesOption);
        command.Options.Add(colorPaletteOption);
        command.Options.Add(colorPaletteMembersOption);
        command.Options.Add(styleCodesOption);
        command.Options.Add(styleTypeOption);
        command.Options.Add(stylePresetOption);
        command.Options.Add(styleReferenceImagesOption);
        command.Options.Add(characterReferenceImagesOption);
        command.Options.Add(characterReferenceImageMasksOption);
        command.Options.Add(downloadOption);

        command.SetAction((parseResult, cancellationToken) =>
        {
            var options = new RemixSampleOptions
            {
                Image = IdeogramFile.FromPath(parseResult.GetValue(imageOption)!),
                Prompt = parseResult.GetValue(promptOption)!,
                ImageWeight = parseResult.GetValue(imageWeightOption),
                Seed = parseResult.GetValue(seedOption),
                Resolution = parseResult.GetValue(resolutionOption),
                AspectRatio = parseResult.GetValue(aspectRatioOption),
                RenderingSpeed = parseResult.GetValue(renderingSpeedOption),
                MagicPrompt = parseResult.GetValue(magicPromptOption),
                NegativePrompt = parseResult.GetValue(negativePromptOption),
                NumImages = parseResult.GetValue(numImagesOption),
                ColorPalette = ParseColorPalette(parseResult.GetValue(colorPaletteOption), parseResult.GetValue(colorPaletteMembersOption)),
                StyleCodes = ParseStringList(parseResult.GetValue(styleCodesOption)),
                StyleType = parseResult.GetValue(styleTypeOption),
                StylePreset = parseResult.GetValue(stylePresetOption),
                StyleReferenceImages = ParseImageFiles(parseResult.GetValue(styleReferenceImagesOption)),
                CharacterReferenceImages = ParseImageFiles(parseResult.GetValue(characterReferenceImagesOption)),
                CharacterReferenceImageMasks = ParseImageFiles(parseResult.GetValue(characterReferenceImageMasksOption))
            };

            return ExecuteSampleCommandAsync(
                parseResult.GetValue(apiKeyOption),
                "remix",
                parseResult.GetValue(downloadOption),
                (client, token) => IdeogramClientSamples.RemixAsync(client, options, token),
                cancellationToken);
        });

        return command;
    }

    private static Command CreateReframeCommand(Option<string?> apiKeyOption)
    {
        var imageOption = CreateRequiredStringOption("--image", "Input image path.");
        var resolutionOption = CreateRequiredStringOption("--resolution", "Target resolution.");
        var numImagesOption = CreateIntOption("--num-images", "Number of images.");
        var seedOption = CreateIntOption("--seed", "Seed value.");
        var renderingSpeedOption = CreateStringOption("--rendering-speed", "Rendering speed.");
        var stylePresetOption = CreateStringOption("--style-preset", "Style preset.");
        var colorPaletteOption = CreateStringOption("--color-palette", "Color palette preset.");
        var colorPaletteMembersOption = CreateStringOption("--color-palette-members", "Semicolon-separated #RRGGBB[:weight] members.");
        var styleCodesOption = CreateStringOption("--style-codes", "Semicolon-separated style codes.");
        var styleReferenceImagesOption = CreateStringOption("--style-reference-images", "Semicolon-separated style reference image paths.");
        var downloadOption = CreateDownloadOption();

        var command = new Command("reframe", "Run the reframe sample.");
        command.Options.Add(imageOption);
        command.Options.Add(resolutionOption);
        command.Options.Add(numImagesOption);
        command.Options.Add(seedOption);
        command.Options.Add(renderingSpeedOption);
        command.Options.Add(stylePresetOption);
        command.Options.Add(colorPaletteOption);
        command.Options.Add(colorPaletteMembersOption);
        command.Options.Add(styleCodesOption);
        command.Options.Add(styleReferenceImagesOption);
        command.Options.Add(downloadOption);

        command.SetAction((parseResult, cancellationToken) =>
        {
            var options = new ReframeSampleOptions
            {
                Image = IdeogramFile.FromPath(parseResult.GetValue(imageOption)!),
                Resolution = parseResult.GetValue(resolutionOption)!,
                NumImages = parseResult.GetValue(numImagesOption),
                Seed = parseResult.GetValue(seedOption),
                RenderingSpeed = parseResult.GetValue(renderingSpeedOption),
                StylePreset = parseResult.GetValue(stylePresetOption),
                ColorPalette = ParseColorPalette(parseResult.GetValue(colorPaletteOption), parseResult.GetValue(colorPaletteMembersOption)),
                StyleCodes = ParseStringList(parseResult.GetValue(styleCodesOption)),
                StyleReferenceImages = ParseImageFiles(parseResult.GetValue(styleReferenceImagesOption))
            };

            return ExecuteSampleCommandAsync(
                parseResult.GetValue(apiKeyOption),
                "reframe",
                parseResult.GetValue(downloadOption),
                (client, token) => IdeogramClientSamples.ReframeAsync(client, options, token),
                cancellationToken);
        });

        return command;
    }

    private static Command CreateReplaceBackgroundCommand(Option<string?> apiKeyOption)
    {
        var imageOption = CreateRequiredStringOption("--image", "Input image path.");
        var promptOption = CreateRequiredStringOption("--prompt", "Prompt text.");
        var magicPromptOption = CreateStringOption("--magic-prompt", "Magic prompt option.");
        var numImagesOption = CreateIntOption("--num-images", "Number of images.");
        var seedOption = CreateIntOption("--seed", "Seed value.");
        var renderingSpeedOption = CreateStringOption("--rendering-speed", "Rendering speed.");
        var stylePresetOption = CreateStringOption("--style-preset", "Style preset.");
        var colorPaletteOption = CreateStringOption("--color-palette", "Color palette preset.");
        var colorPaletteMembersOption = CreateStringOption("--color-palette-members", "Semicolon-separated #RRGGBB[:weight] members.");
        var styleCodesOption = CreateStringOption("--style-codes", "Semicolon-separated style codes.");
        var styleReferenceImagesOption = CreateStringOption("--style-reference-images", "Semicolon-separated style reference image paths.");
        var downloadOption = CreateDownloadOption();

        var command = new Command("replace-background", "Run the replace-background sample.");
        command.Options.Add(imageOption);
        command.Options.Add(promptOption);
        command.Options.Add(magicPromptOption);
        command.Options.Add(numImagesOption);
        command.Options.Add(seedOption);
        command.Options.Add(renderingSpeedOption);
        command.Options.Add(stylePresetOption);
        command.Options.Add(colorPaletteOption);
        command.Options.Add(colorPaletteMembersOption);
        command.Options.Add(styleCodesOption);
        command.Options.Add(styleReferenceImagesOption);
        command.Options.Add(downloadOption);

        command.SetAction((parseResult, cancellationToken) =>
        {
            var options = new ReplaceBackgroundSampleOptions
            {
                Image = IdeogramFile.FromPath(parseResult.GetValue(imageOption)!),
                Prompt = parseResult.GetValue(promptOption)!,
                MagicPrompt = parseResult.GetValue(magicPromptOption),
                NumImages = parseResult.GetValue(numImagesOption),
                Seed = parseResult.GetValue(seedOption),
                RenderingSpeed = parseResult.GetValue(renderingSpeedOption),
                StylePreset = parseResult.GetValue(stylePresetOption),
                ColorPalette = ParseColorPalette(parseResult.GetValue(colorPaletteOption), parseResult.GetValue(colorPaletteMembersOption)),
                StyleCodes = ParseStringList(parseResult.GetValue(styleCodesOption)),
                StyleReferenceImages = ParseImageFiles(parseResult.GetValue(styleReferenceImagesOption))
            };

            return ExecuteSampleCommandAsync(
                parseResult.GetValue(apiKeyOption),
                "replace-background",
                parseResult.GetValue(downloadOption),
                (client, token) => IdeogramClientSamples.ReplaceBackgroundAsync(client, options, token),
                cancellationToken);
        });

        return command;
    }

    private static Command CreateDownloadCommand(Option<string?> apiKeyOption)
    {
        var urlOption = CreateRequiredStringOption("--url", "Image URL to download.");
        var outputOption = CreateStringOption("--output", "Explicit output path.");

        var command = new Command("download", "Download a single image URL.");
        command.Options.Add(urlOption);
        command.Options.Add(outputOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var outputDirectory = OutputWriter.CreateOutputDirectory();
            var imageUrl = parseResult.GetValue(urlOption)!;
            var outputPath = parseResult.GetValue(outputOption) ??
                             Path.Combine(outputDirectory, $"downloaded.{DetermineExtensionToken(imageUrl)}");

            using var client = CreateClient(parseResult.GetValue(apiKeyOption));

            try
            {
                var savedPath = await client.DownloadImageAsync(imageUrl, outputPath, cancellationToken).ConfigureAwait(false);
                OutputWriter.PrintSavedImagePaths([savedPath]);
                return 0;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return 1;
            }
        });

        return command;
    }

    private static IdeogramClient CreateClient(string? commandLineApiKey)
    {
        return new IdeogramClient(new IdeogramClientOptions
        {
            ApiKey = ResolveApiKey(commandLineApiKey)
        });
    }

    private static async Task<int> ExecuteSampleCommandAsync(
        string? commandLineApiKey,
        string methodName,
        bool shouldDownload,
        Func<IIdeogramClient, CancellationToken, Task<IdeogramResponse>> executeAsync,
        CancellationToken cancellationToken)
    {
        var outputDirectory = OutputWriter.CreateOutputDirectory();

        using var client = CreateClient(commandLineApiKey);

        try
        {
            var response = await executeAsync(client, cancellationToken).ConfigureAwait(false);
            OutputWriter.PrintResponseSummary(response);
            var responsePath = OutputWriter.SaveResponseJson(outputDirectory, methodName, response);
            OutputWriter.PrintSavedResponsePath(responsePath);

            if (shouldDownload)
            {
                var savedImages = await client.DownloadImagesAsync(response, outputDirectory, methodName, cancellationToken).ConfigureAwait(false);
                OutputWriter.PrintSavedImagePaths(savedImages);
            }

            return 0;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
            return 1;
        }
    }

    private static string ResolveApiKey(string? commandLineApiKey)
    {
        var apiKey = commandLineApiKey
                     ?? Environment.GetEnvironmentVariable("IDEOGRAM_API_KEY")
                     ?? TryReadApiKeyFromUserSecrets();

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }

        throw new InvalidOperationException(
            "Ideogram API key was not found. Provide --api-key, set IDEOGRAM_API_KEY, or configure User Secrets.");
    }

    private static string? TryReadApiKeyFromUserSecrets()
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly, optional: true)
                .Build();

            var apiKey = configuration["Ideogram:ApiKey"] ?? configuration["IdeogramApiKey"];
            return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException)
        {
            throw new InvalidOperationException($"Failed to read User Secrets: {ex.Message}", ex);
        }
    }

    private static async Task RunInteractiveMenuAsync(IIdeogramClient client, string outputDirectory, CancellationToken cancellationToken)
    {
        while (true)
        {
            System.Console.WriteLine("Ideogram.Client Samples");
            System.Console.WriteLine();
            System.Console.WriteLine("API key: loaded");
            System.Console.WriteLine($"Output directory: ./{outputDirectory.Replace('\\', '/')}");
            System.Console.WriteLine();
            System.Console.WriteLine("Select sample:");
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
                    await RunGenerateInteractiveAsync(client, outputDirectory, cancellationToken).ConfigureAwait(false);
                    break;
                case "2":
                    await RunGenerateTransparentInteractiveAsync(client, outputDirectory, cancellationToken).ConfigureAwait(false);
                    break;
                case "3":
                    System.Console.WriteLine("Mask must be the same dimensions as the image. Per Ideogram docs, black mask regions indicate the regions to edit.");
                    await RunInpaintInteractiveAsync(client, outputDirectory, cancellationToken).ConfigureAwait(false);
                    break;
                case "4":
                    await RunRemixInteractiveAsync(client, outputDirectory, cancellationToken).ConfigureAwait(false);
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
                    await RunReframeInteractiveAsync(client, outputDirectory, cancellationToken).ConfigureAwait(false);
                    break;
                case "6":
                    await RunReplaceBackgroundInteractiveAsync(client, outputDirectory, cancellationToken).ConfigureAwait(false);
                    break;
                case "7":
                    await DownloadSingleImageInteractiveAsync(client, outputDirectory, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    System.Console.WriteLine("Unknown selection.");
                    break;
            }

            System.Console.WriteLine();
        }
    }

    private static Task RunGenerateInteractiveAsync(IIdeogramClient client, string outputDirectory, CancellationToken cancellationToken)
    {
        var options = new GenerateSampleOptions
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

        return ExecuteInteractiveSampleAsync(
            client,
            outputDirectory,
            "generate",
            (sampleClient, token) => IdeogramClientSamples.GenerateAsync(sampleClient, options, token),
            cancellationToken);
    }

    private static Task RunGenerateTransparentInteractiveAsync(IIdeogramClient client, string outputDirectory, CancellationToken cancellationToken)
    {
        var options = new GenerateTransparentSampleOptions
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

        return ExecuteInteractiveSampleAsync(
            client,
            outputDirectory,
            "transparent",
            (sampleClient, token) => IdeogramClientSamples.GenerateTransparentAsync(sampleClient, options, token),
            cancellationToken);
    }

    private static Task RunInpaintInteractiveAsync(IIdeogramClient client, string outputDirectory, CancellationToken cancellationToken)
    {
        var options = new InpaintSampleOptions
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

        return ExecuteInteractiveSampleAsync(
            client,
            outputDirectory,
            "inpaint",
            (sampleClient, token) => IdeogramClientSamples.InpaintAsync(sampleClient, options, token),
            cancellationToken);
    }

    private static Task RunRemixInteractiveAsync(IIdeogramClient client, string outputDirectory, CancellationToken cancellationToken)
    {
        var options = new RemixSampleOptions
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

        return ExecuteInteractiveSampleAsync(
            client,
            outputDirectory,
            "remix",
            (sampleClient, token) => IdeogramClientSamples.RemixAsync(sampleClient, options, token),
            cancellationToken);
    }

    private static Task RunReframeInteractiveAsync(IIdeogramClient client, string outputDirectory, CancellationToken cancellationToken)
    {
        var options = new ReframeSampleOptions
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

        return ExecuteInteractiveSampleAsync(
            client,
            outputDirectory,
            "reframe",
            (sampleClient, token) => IdeogramClientSamples.ReframeAsync(sampleClient, options, token),
            cancellationToken);
    }

    private static Task RunReplaceBackgroundInteractiveAsync(IIdeogramClient client, string outputDirectory, CancellationToken cancellationToken)
    {
        var options = new ReplaceBackgroundSampleOptions
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

        return ExecuteInteractiveSampleAsync(
            client,
            outputDirectory,
            "replace-background",
            (sampleClient, token) => IdeogramClientSamples.ReplaceBackgroundAsync(sampleClient, options, token),
            cancellationToken);
    }

    private static async Task ExecuteInteractiveSampleAsync(
        IIdeogramClient client,
        string outputDirectory,
        string methodName,
        Func<IIdeogramClient, CancellationToken, Task<IdeogramResponse>> executeAsync,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await executeAsync(client, cancellationToken).ConfigureAwait(false);
            OutputWriter.PrintResponseSummary(response);
            var responsePath = OutputWriter.SaveResponseJson(outputDirectory, methodName, response);
            OutputWriter.PrintSavedResponsePath(responsePath);

            if (ConsolePrompts.Confirm("Download returned images now?", defaultValue: true))
            {
                var savedImages = await client.DownloadImagesAsync(response, outputDirectory, methodName, cancellationToken).ConfigureAwait(false);
                OutputWriter.PrintSavedImagePaths(savedImages);
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }
    }

    private static async Task DownloadSingleImageInteractiveAsync(IIdeogramClient client, string outputDirectory, CancellationToken cancellationToken)
    {
        var imageUrl = ConsolePrompts.RequiredString("Image URL");
        var outputPath = ConsolePrompts.OptionalPath("Output path") ??
                         Path.Combine(outputDirectory, $"downloaded.{DetermineExtensionToken(imageUrl)}");

        try
        {
            var savedPath = await client.DownloadImageAsync(imageUrl, outputPath, cancellationToken).ConfigureAwait(false);
            OutputWriter.PrintSavedImagePaths([savedPath]);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }
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

    private static ColorPalette? ParseColorPalette(string? preset, string? membersInput)
    {
        if (!string.IsNullOrWhiteSpace(preset))
        {
            return ColorPalette.FromPreset(preset);
        }

        var members = ParseStringList(membersInput);
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
        return paths?.Select(static path => IdeogramFile.FromPath(path)).ToArray();
    }

    private static IReadOnlyList<IdeogramFile>? ParseImageUrls(string? input)
    {
        var urls = ParseStringList(input);
        return urls?.Select(static url => IdeogramFile.FromUrl(url)).ToArray();
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

    private static Option<string?> CreateStringOption(string name, string description)
    {
        return new Option<string?>(name)
        {
            Description = description
        };
    }

    private static Option<string?> CreateRequiredStringOption(string name, string description)
    {
        return new Option<string?>(name)
        {
            Description = description,
            Required = true
        };
    }

    private static Option<int?> CreateIntOption(string name, string description)
    {
        return new Option<int?>(name)
        {
            Description = description
        };
    }

    private static Option<bool> CreateDownloadOption()
    {
        return new Option<bool>("--download")
        {
            Description = "Download returned images immediately."
        };
    }
}
