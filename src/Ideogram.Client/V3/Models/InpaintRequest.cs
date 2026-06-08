namespace A2G.Ideogram.Client.Models;

public sealed class InpaintRequest
{
    public required IdeogramFile Image { get; init; }

    public required IdeogramFile Mask { get; init; }

    public required string Prompt { get; init; }

    public string? MagicPrompt { get; init; }

    public int? NumImages { get; init; }

    public int? Seed { get; init; }

    public string? RenderingSpeed { get; init; }

    public string? StyleType { get; init; }

    public string? StylePreset { get; init; }

    public ColorPalette? ColorPalette { get; init; }

    public IReadOnlyList<string>? StyleCodes { get; init; }

    public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }

    public IReadOnlyList<IdeogramFile>? CharacterReferenceImages { get; init; }

    public IReadOnlyList<IdeogramFile>? CharacterReferenceImageMasks { get; init; }
}
