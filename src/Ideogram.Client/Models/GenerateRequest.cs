using System.Collections.Generic;

namespace Ideogram.Client.Models;

public sealed class GenerateRequest
{
    public required string Prompt { get; init; }

    public int? Seed { get; init; }

    public string? Resolution { get; init; }

    public string? AspectRatio { get; init; }

    public string? RenderingSpeed { get; init; }

    public string? MagicPrompt { get; init; }

    public string? NegativePrompt { get; init; }

    public int? NumImages { get; init; }

    public ColorPalette? ColorPalette { get; init; }

    public IReadOnlyList<string>? StyleCodes { get; init; }

    public string? StyleType { get; init; }

    public string? StylePreset { get; init; }

    public string? CustomModelUri { get; init; }

    public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }

    public IReadOnlyList<IdeogramFile>? CharacterReferenceImages { get; init; }

    public IReadOnlyList<IdeogramFile>? CharacterReferenceImageMasks { get; init; }
}
