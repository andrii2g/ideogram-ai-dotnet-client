namespace A2G.Ideogram.Client.Models;

public sealed class ReframeRequest
{
    public required IdeogramFile Image { get; init; }

    public required string Resolution { get; init; }

    public int? NumImages { get; init; }

    public int? Seed { get; init; }

    public string? RenderingSpeed { get; init; }

    public string? StylePreset { get; init; }

    public ColorPalette? ColorPalette { get; init; }

    public IReadOnlyList<string>? StyleCodes { get; init; }

    public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }
}
