namespace A2G.Ideogram.Client.V4.Models;

public sealed class GenerateFromJsonRequest
{
    public required JsonPrompt JsonPrompt { get; init; }

    public string? Resolution { get; init; }

    public string? RenderingSpeed { get; init; }

    public bool? EnableCopyrightDetection { get; init; }
}
