using A2G.Ideogram.Client.Constants;
using A2G.Ideogram.Client.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace A2G.Ideogram.Client.Samples;

internal static class Program
{
    /// <summary>
    /// manually change the sample scenario to run different samples. 
    /// </summary>
    private const SampleScenario SelectedSample = SampleScenario.Generate;
    // set to true to automatically download returned images to the output directory
    private static readonly bool DownloadReturnedImages = true;

    private static async Task Main()
    {
        var outputDirectory = CreateOutputDirectory();

        using var client = new IdeogramClient(new IdeogramClientOptions
        {
            ApiKey = ResolveApiKey()
        });

        var response = await ExecuteSelectedSampleAsync(client).ConfigureAwait(false);

        WriteResponseSummary(response);
        var responsePath = SaveResponseJson(outputDirectory, SelectedSample.ToString(), response);
        Console.WriteLine($"Saved response: {responsePath}");

        if (DownloadReturnedImages)
        {
            var savedImages = await client.DownloadImagesAsync(
                response,
                outputDirectory,
                SelectedSample.ToString().ToLowerInvariant()).ConfigureAwait(false);

            Console.WriteLine("Saved images:");
            foreach (var path in savedImages)
            {
                Console.WriteLine($"  {path}");
            }
        }
    }

    private static Task<IdeogramResponse> ExecuteSelectedSampleAsync(IIdeogramClient client)
    {
        return SelectedSample switch
        {
            SampleScenario.Generate => RunGenerateSampleAsync(client),
            SampleScenario.GenerateTransparent => RunGenerateTransparentSampleAsync(client),
            SampleScenario.Inpaint => RunInpaintSampleAsync(client),
            SampleScenario.Remix => RunRemixSampleAsync(client),
            SampleScenario.Reframe => RunReframeSampleAsync(client),
            SampleScenario.ReplaceBackground => RunReplaceBackgroundSampleAsync(client),
            _ => throw new NotSupportedException($"Unsupported sample scenario '{SelectedSample}'.")
        };
    }

    private static Task<IdeogramResponse> RunGenerateSampleAsync(IIdeogramClient client)
    {
        return client.GenerateAsync(new GenerateRequest
        {
            Prompt = "A cinematic product photo of a ceramic coffee mug on a wooden desk.",
            RenderingSpeed = IdeogramRenderingSpeed.Turbo,
            NumImages = 1,
            StyleType = IdeogramStyleTypes.Realistic,
            AspectRatio = IdeogramAspectRatios.Ratio1x1
        });
    }

    private static Task<IdeogramResponse> RunGenerateTransparentSampleAsync(IIdeogramClient client)
    {
        return client.GenerateTransparentAsync(new GenerateTransparentRequest
        {
            Prompt = "A clean flat logo of a coffee bean with transparent background.",
            RenderingSpeed = IdeogramRenderingSpeed.Turbo,
            NumImages = 1,
            UpscaleFactor = IdeogramUpscaleFactors.X1,
            AspectRatio = IdeogramAspectRatios.Ratio1x1
        });
    }

    private static Task<IdeogramResponse> RunInpaintSampleAsync(IIdeogramClient client)
    {
        return client.InpaintAsync(new InpaintRequest
        {
            Image = IdeogramFile.FromPath(@"manual-assets\image.png"),
            Mask = IdeogramFile.FromPath(@"manual-assets\mask.png"),
            Prompt = "Replace the masked area with a red scarf.",
            RenderingSpeed = IdeogramRenderingSpeed.Default,
            NumImages = 1
        });
    }

    private static Task<IdeogramResponse> RunRemixSampleAsync(IIdeogramClient client)
    {
        return client.RemixAsync(new RemixRequest
        {
            Image = IdeogramFile.FromPath(@"manual-assets\image.png"),
            Prompt = "Transform this into a watercolor illustration.",
            ImageWeight = 50,
            RenderingSpeed = IdeogramRenderingSpeed.Turbo,
            NumImages = 1
        });
    }

    private static Task<IdeogramResponse> RunReframeSampleAsync(IIdeogramClient client)
    {
        return client.ReframeAsync(new ReframeRequest
        {
            Image = IdeogramFile.FromPath(@"manual-assets\square.png"),
            Resolution = "1312x736",
            RenderingSpeed = IdeogramRenderingSpeed.Default,
            NumImages = 1
        });
    }

    private static Task<IdeogramResponse> RunReplaceBackgroundSampleAsync(IIdeogramClient client)
    {
        return client.ReplaceBackgroundAsync(new ReplaceBackgroundRequest
        {
            Image = IdeogramFile.FromPath(@"manual-assets\person.png"),
            Prompt = "Place the subject in a modern coworking office.",
            RenderingSpeed = IdeogramRenderingSpeed.Default,
            NumImages = 1
        });
    }

    private static string ResolveApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("IDEOGRAM_API_KEY") ?? TryReadApiKeyFromUserSecrets();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }

        throw new InvalidOperationException(
            "Ideogram API key was not found. Set IDEOGRAM_API_KEY or configure User Secrets for this samples project.");
    }

    private static string? TryReadApiKeyFromUserSecrets()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(Program).Assembly, optional: true)
            .Build();

        var apiKey = configuration["Ideogram:ApiKey"] ?? configuration["IdeogramApiKey"];
        return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey;
    }

    private static string CreateOutputDirectory()
    {
        var outputDirectory = Path.Combine("outputs", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(outputDirectory);
        return outputDirectory;
    }

    private static string SaveResponseJson(string outputDirectory, string sampleName, IdeogramResponse response)
    {
        var filePath = Path.Combine(outputDirectory, $"{sampleName.ToLowerInvariant()}_response.json");
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
        return filePath;
    }

    private static void WriteResponseSummary(IdeogramResponse response)
    {
        Console.WriteLine($"Created: {response.Created ?? "(not provided)"}");
        Console.WriteLine($"Images returned: {response.Data.Count}");
        Console.WriteLine();

        for (var i = 0; i < response.Data.Count; i++)
        {
            var image = response.Data[i];
            Console.WriteLine($"[{i}]");
            Console.WriteLine($"  Safe: {image.IsImageSafe?.ToString() ?? "(not provided)"}");
            Console.WriteLine($"  Seed: {image.Seed?.ToString() ?? "(not provided)"}");
            Console.WriteLine($"  Resolution: {image.Resolution ?? "(not provided)"}");
            Console.WriteLine($"  URL: {image.Url ?? "(not provided)"}");
            Console.WriteLine();
        }
    }
}

internal enum SampleScenario
{
    Generate,
    GenerateTransparent,
    Inpaint,
    Remix,
    Reframe,
    ReplaceBackground
}
