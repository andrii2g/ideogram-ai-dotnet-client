namespace A2G.Ideogram.Client.V4.Models;

public sealed class GenerateFromTextRequest
{
    public required string TextPrompt { get; init; }

    public string? Resolution { get; init; }

    public string? RenderingSpeed { get; init; }

    public bool? EnableCopyrightDetection { get; init; }
}
