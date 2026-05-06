using A2G.Ideogram.Client.Constants;
using A2G.Ideogram.Client.Models;

namespace A2G.Ideogram.Client.ConsoleApp;

internal static class IdeogramClientSamples
{
    public static Task<IdeogramResponse> GenerateAsync(
        IIdeogramClient client,
        GenerateSampleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        return client.GenerateAsync(new GenerateRequest
        {
            Prompt = options.Prompt,
            Seed = options.Seed,
            Resolution = options.Resolution,
            AspectRatio = options.AspectRatio,
            RenderingSpeed = options.RenderingSpeed ?? IdeogramRenderingSpeed.Turbo,
            MagicPrompt = options.MagicPrompt,
            NegativePrompt = options.NegativePrompt,
            NumImages = options.NumImages ?? 1,
            ColorPalette = options.ColorPalette,
            StyleCodes = options.StyleCodes,
            StyleType = options.StyleType ?? IdeogramStyleTypes.General,
            StylePreset = options.StylePreset,
            CustomModelUri = options.CustomModelUri,
            StyleReferenceImages = options.StyleReferenceImages,
            CharacterReferenceImages = options.CharacterReferenceImages,
            CharacterReferenceImageMasks = options.CharacterReferenceImageMasks
        }, cancellationToken);
    }

    public static Task<IdeogramResponse> GenerateTransparentAsync(
        IIdeogramClient client,
        GenerateTransparentSampleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        return client.GenerateTransparentAsync(new GenerateTransparentRequest
        {
            Prompt = options.Prompt,
            Seed = options.Seed,
            UpscaleFactor = options.UpscaleFactor ?? IdeogramUpscaleFactors.X1,
            AspectRatio = options.AspectRatio,
            RenderingSpeed = options.RenderingSpeed ?? IdeogramRenderingSpeed.Turbo,
            MagicPrompt = options.MagicPrompt,
            NegativePrompt = options.NegativePrompt,
            NumImages = options.NumImages ?? 1
        }, cancellationToken);
    }

    public static Task<IdeogramResponse> InpaintAsync(
        IIdeogramClient client,
        InpaintSampleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        return client.InpaintAsync(new InpaintRequest
        {
            Image = options.Image,
            Mask = options.Mask,
            Prompt = options.Prompt,
            MagicPrompt = options.MagicPrompt,
            NumImages = options.NumImages ?? 1,
            Seed = options.Seed,
            RenderingSpeed = options.RenderingSpeed ?? IdeogramRenderingSpeed.Default,
            StyleType = options.StyleType,
            StylePreset = options.StylePreset,
            ColorPalette = options.ColorPalette,
            StyleCodes = options.StyleCodes,
            StyleReferenceImages = options.StyleReferenceImages,
            CharacterReferenceImages = options.CharacterReferenceImages,
            CharacterReferenceImageMasks = options.CharacterReferenceImageMasks
        }, cancellationToken);
    }

    public static Task<IdeogramResponse> RemixAsync(
        IIdeogramClient client,
        RemixSampleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        return client.RemixAsync(new RemixRequest
        {
            Image = options.Image,
            Prompt = options.Prompt,
            ImageWeight = options.ImageWeight ?? 50,
            Seed = options.Seed,
            Resolution = options.Resolution,
            AspectRatio = options.AspectRatio,
            RenderingSpeed = options.RenderingSpeed ?? IdeogramRenderingSpeed.Turbo,
            MagicPrompt = options.MagicPrompt,
            NegativePrompt = options.NegativePrompt,
            NumImages = options.NumImages ?? 1,
            ColorPalette = options.ColorPalette,
            StyleCodes = options.StyleCodes,
            StyleType = options.StyleType ?? IdeogramStyleTypes.General,
            StylePreset = options.StylePreset,
            StyleReferenceImages = options.StyleReferenceImages,
            CharacterReferenceImages = options.CharacterReferenceImages,
            CharacterReferenceImageMasks = options.CharacterReferenceImageMasks
        }, cancellationToken);
    }

    public static Task<IdeogramResponse> ReframeAsync(
        IIdeogramClient client,
        ReframeSampleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        return client.ReframeAsync(new ReframeRequest
        {
            Image = options.Image,
            Resolution = options.Resolution,
            NumImages = options.NumImages ?? 1,
            Seed = options.Seed,
            RenderingSpeed = options.RenderingSpeed ?? IdeogramRenderingSpeed.Default,
            StylePreset = options.StylePreset,
            ColorPalette = options.ColorPalette,
            StyleCodes = options.StyleCodes,
            StyleReferenceImages = options.StyleReferenceImages
        }, cancellationToken);
    }

    public static Task<IdeogramResponse> ReplaceBackgroundAsync(
        IIdeogramClient client,
        ReplaceBackgroundSampleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        return client.ReplaceBackgroundAsync(new ReplaceBackgroundRequest
        {
            Image = options.Image,
            Prompt = options.Prompt,
            MagicPrompt = options.MagicPrompt,
            NumImages = options.NumImages ?? 1,
            Seed = options.Seed,
            RenderingSpeed = options.RenderingSpeed ?? IdeogramRenderingSpeed.Default,
            StylePreset = options.StylePreset,
            ColorPalette = options.ColorPalette,
            StyleCodes = options.StyleCodes,
            StyleReferenceImages = options.StyleReferenceImages
        }, cancellationToken);
    }
}

internal sealed class GenerateSampleOptions
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

internal sealed class GenerateTransparentSampleOptions
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

internal sealed class InpaintSampleOptions
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

internal sealed class RemixSampleOptions
{
    public required IdeogramFile Image { get; init; }
    public required string Prompt { get; init; }
    public int? ImageWeight { get; init; }
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
    public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }
    public IReadOnlyList<IdeogramFile>? CharacterReferenceImages { get; init; }
    public IReadOnlyList<IdeogramFile>? CharacterReferenceImageMasks { get; init; }
}

internal sealed class ReframeSampleOptions
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

internal sealed class ReplaceBackgroundSampleOptions
{
    public required IdeogramFile Image { get; init; }
    public required string Prompt { get; init; }
    public string? MagicPrompt { get; init; }
    public int? NumImages { get; init; }
    public int? Seed { get; init; }
    public string? RenderingSpeed { get; init; }
    public string? StylePreset { get; init; }
    public ColorPalette? ColorPalette { get; init; }
    public IReadOnlyList<string>? StyleCodes { get; init; }
    public IReadOnlyList<IdeogramFile>? StyleReferenceImages { get; init; }
}
