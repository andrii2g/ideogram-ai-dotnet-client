using A2G.Ideogram.Client.Models;

namespace A2G.Ideogram.Client.V4.Models;

public sealed class RemixRequest
{
    public required IdeogramFile Image { get; init; }

    public required string TextPrompt { get; init; }

    public int? ImageWeight { get; init; }

    public string? Resolution { get; init; }

    public string? RenderingSpeed { get; init; }

    public bool? EnableCopyrightDetection { get; init; }
}
