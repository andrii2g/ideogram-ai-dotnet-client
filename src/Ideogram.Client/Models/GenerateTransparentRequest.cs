namespace Ideogram.Client.Models;

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
