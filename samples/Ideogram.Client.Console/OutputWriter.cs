using System.Text.Json;
using A2G.Ideogram.Client.Models;

namespace A2G.Ideogram.Client.ConsoleApp;

internal static class OutputWriter
{
    public static string CreateOutputDirectory()
    {
        var outputDirectory = Path.Combine("outputs", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(outputDirectory);
        return outputDirectory;
    }

    public static string SaveResponseJson(string outputDirectory, string methodName, IdeogramResponse response)
    {
        var filePath = Path.Combine(outputDirectory, $"{methodName}_response.json");
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
        return filePath;
    }

    public static void PrintResponseSummary(IdeogramResponse response)
    {
        System.Console.WriteLine("Request succeeded.");
        System.Console.WriteLine($"Created: {response.Created ?? "(not provided)"}");
        System.Console.WriteLine($"Images returned: {response.Data.Count}");
        System.Console.WriteLine();

        for (var i = 0; i < response.Data.Count; i++)
        {
            var image = response.Data[i];
            System.Console.WriteLine($"[{i}]");
            System.Console.WriteLine($"  Safe: {image.IsImageSafe?.ToString() ?? "(not provided)"}");
            System.Console.WriteLine($"  Seed: {image.Seed?.ToString() ?? "(not provided)"}");
            System.Console.WriteLine($"  Resolution: {image.Resolution ?? "(not provided)"}");
            System.Console.WriteLine($"  URL: {image.Url ?? "(not provided)"}");
            System.Console.WriteLine();
        }
    }

    public static void PrintSavedResponsePath(string responsePath)
    {
        System.Console.WriteLine("Saved response:");
        System.Console.WriteLine($"  {responsePath}");
        System.Console.WriteLine();
    }

    public static void PrintSavedImagePaths(IReadOnlyList<string> savedPaths)
    {
        if (savedPaths.Count == 0)
        {
            System.Console.WriteLine("No images were downloaded.");
            return;
        }

        System.Console.WriteLine("Saved images:");
        foreach (var path in savedPaths)
        {
            System.Console.WriteLine($"  {path}");
        }

        System.Console.WriteLine();
    }

    public static void PrintHelp()
    {
        System.Console.WriteLine("Supported modes:");
        System.Console.WriteLine("  generate");
        System.Console.WriteLine("  transparent");
        System.Console.WriteLine("  inpaint");
        System.Console.WriteLine("  remix");
        System.Console.WriteLine("  reframe");
        System.Console.WriteLine("  replace-background");
        System.Console.WriteLine("  download");
        System.Console.WriteLine();
        System.Console.WriteLine("Examples:");
        System.Console.WriteLine("  dotnet run --project samples/Ideogram.Client.Console -- generate --prompt \"A photo of a cat sleeping on a couch.\" --rendering-speed TURBO --num-images 1 --download true");
        System.Console.WriteLine("  dotnet run --project samples/Ideogram.Client.Console -- transparent --prompt \"A clean logo for Ideogram Coffee\" --aspect-ratio 1x1 --upscale-factor X2");
        System.Console.WriteLine("  dotnet run --project samples/Ideogram.Client.Console -- inpaint --image ./input/cat.png --mask ./input/mask.png --prompt \"A photo of a cat wearing a hat.\"");
        System.Console.WriteLine("  dotnet run --project samples/Ideogram.Client.Console -- remix --image ./input/cat.png --prompt \"A photo of a dog sleeping on a couch\" --image-weight 60");
        System.Console.WriteLine("  dotnet run --project samples/Ideogram.Client.Console -- reframe --image ./input/square.png --resolution 1312x736");
        System.Console.WriteLine("  dotnet run --project samples/Ideogram.Client.Console -- replace-background --image ./input/person.png --prompt \"A busy coffee shop in the background\"");
        System.Console.WriteLine();
    }
}
