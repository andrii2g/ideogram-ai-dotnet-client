using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Ideogram.Client.Constants;
using Ideogram.Client.Internal;
using Ideogram.Client.Models;

namespace Ideogram.Client.Validation;

internal static partial class RequestValidator
{
    private const long MaxImageBytes = 10_000_000;

    public static void Validate(GenerateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidatePrompt(request.Prompt, nameof(GenerateRequest));
        ValidateSeed(request.Seed, nameof(GenerateRequest));
        ValidateResolutionAndAspectRatio(request.Resolution, request.AspectRatio, nameof(GenerateRequest));
        ValidateRenderingSpeed(request.RenderingSpeed, nameof(GenerateRequest));
        ValidateMagicPrompt(request.MagicPrompt, nameof(GenerateRequest));
        ValidateNumImages(request.NumImages, nameof(GenerateRequest));
        ValidateColorPalette(request.ColorPalette);
        ValidateStyleCodes(request.StyleCodes, request.StyleReferenceImages, request.StyleType, nameof(GenerateRequest));
        ValidateStyleType(request.StyleType, nameof(GenerateRequest));
        ValidateStylePreset(request.StylePreset, nameof(GenerateRequest));
        ValidateFilesTotal(request.StyleReferenceImages, "GenerateRequest.StyleReferenceImages");
        ValidateFilesTotal(request.CharacterReferenceImages, "GenerateRequest.CharacterReferenceImages");
        ValidateFilesTotal(request.CharacterReferenceImageMasks, "GenerateRequest.CharacterReferenceImageMasks");
        ValidateCharacterMaskPairing(
            request.CharacterReferenceImages,
            request.CharacterReferenceImageMasks,
            nameof(GenerateRequest));
    }

    public static void Validate(GenerateTransparentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidatePrompt(request.Prompt, nameof(GenerateTransparentRequest));
        ValidateSeed(request.Seed, nameof(GenerateTransparentRequest));
        ValidateAspectRatio(request.AspectRatio, nameof(GenerateTransparentRequest));
        ValidateRenderingSpeed(request.RenderingSpeed, nameof(GenerateTransparentRequest), allowFlash: false);
        ValidateMagicPrompt(request.MagicPrompt, nameof(GenerateTransparentRequest));
        ValidateNumImages(request.NumImages, nameof(GenerateTransparentRequest));
        ValidateUpscaleFactor(request.UpscaleFactor);
    }

    public static void Validate(InpaintRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequiredFile(request.Image, "InpaintRequest.Image");
        ValidateRequiredFile(request.Mask, "InpaintRequest.Mask");
        ValidatePrompt(request.Prompt, nameof(InpaintRequest));
        ValidateMagicPrompt(request.MagicPrompt, nameof(InpaintRequest));
        ValidateNumImages(request.NumImages, nameof(InpaintRequest));
        ValidateSeed(request.Seed, nameof(InpaintRequest));
        ValidateRenderingSpeed(request.RenderingSpeed, nameof(InpaintRequest));
        ValidateStyleType(request.StyleType, nameof(InpaintRequest));
        ValidateStylePreset(request.StylePreset, nameof(InpaintRequest));
        ValidateColorPalette(request.ColorPalette);
        ValidateStyleCodes(request.StyleCodes, request.StyleReferenceImages, request.StyleType, nameof(InpaintRequest));
        ValidateFilesTotal(request.StyleReferenceImages, "InpaintRequest.StyleReferenceImages");
        ValidateFilesTotal(request.CharacterReferenceImages, "InpaintRequest.CharacterReferenceImages");
        ValidateFilesTotal(request.CharacterReferenceImageMasks, "InpaintRequest.CharacterReferenceImageMasks");
        ValidateCharacterMaskPairing(
            request.CharacterReferenceImages,
            request.CharacterReferenceImageMasks,
            nameof(InpaintRequest));
    }

    public static void Validate(RemixRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequiredFile(request.Image, "RemixRequest.Image");
        ValidatePrompt(request.Prompt, nameof(RemixRequest));
        ValidateImageWeight(request.ImageWeight);
        ValidateSeed(request.Seed, nameof(RemixRequest));
        ValidateResolutionAndAspectRatio(request.Resolution, request.AspectRatio, nameof(RemixRequest));
        ValidateRenderingSpeed(request.RenderingSpeed, nameof(RemixRequest));
        ValidateMagicPrompt(request.MagicPrompt, nameof(RemixRequest));
        ValidateNumImages(request.NumImages, nameof(RemixRequest));
        ValidateColorPalette(request.ColorPalette);
        ValidateStyleCodes(request.StyleCodes, request.StyleReferenceImages, request.StyleType, nameof(RemixRequest));
        ValidateStyleType(request.StyleType, nameof(RemixRequest));
        ValidateStylePreset(request.StylePreset, nameof(RemixRequest));
        ValidateFilesTotal(request.StyleReferenceImages, "RemixRequest.StyleReferenceImages");
        ValidateFilesTotal(request.CharacterReferenceImages, "RemixRequest.CharacterReferenceImages");
        ValidateFilesTotal(request.CharacterReferenceImageMasks, "RemixRequest.CharacterReferenceImageMasks");
        ValidateCharacterMaskPairing(
            request.CharacterReferenceImages,
            request.CharacterReferenceImageMasks,
            nameof(RemixRequest));
    }

    public static void Validate(ReframeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequiredFile(request.Image, "ReframeRequest.Image");
        ValidateRequiredResolution(request.Resolution, nameof(ReframeRequest));
        ValidateNumImages(request.NumImages, nameof(ReframeRequest));
        ValidateSeed(request.Seed, nameof(ReframeRequest));
        ValidateRenderingSpeed(request.RenderingSpeed, nameof(ReframeRequest));
        ValidateStylePreset(request.StylePreset, nameof(ReframeRequest));
        ValidateColorPalette(request.ColorPalette);
        ValidateStyleCodes(request.StyleCodes, request.StyleReferenceImages, styleType: null, nameof(ReframeRequest));
        ValidateFilesTotal(request.StyleReferenceImages, "ReframeRequest.StyleReferenceImages");
    }

    public static void Validate(ReplaceBackgroundRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequiredFile(request.Image, "ReplaceBackgroundRequest.Image");
        ValidatePrompt(request.Prompt, nameof(ReplaceBackgroundRequest));
        ValidateMagicPrompt(request.MagicPrompt, nameof(ReplaceBackgroundRequest));
        ValidateNumImages(request.NumImages, nameof(ReplaceBackgroundRequest));
        ValidateSeed(request.Seed, nameof(ReplaceBackgroundRequest));
        ValidateRenderingSpeed(request.RenderingSpeed, nameof(ReplaceBackgroundRequest));
        ValidateStylePreset(request.StylePreset, nameof(ReplaceBackgroundRequest));
        ValidateColorPalette(request.ColorPalette);
        ValidateStyleCodes(request.StyleCodes, request.StyleReferenceImages, styleType: null, nameof(ReplaceBackgroundRequest));
        ValidateFilesTotal(request.StyleReferenceImages, "ReplaceBackgroundRequest.StyleReferenceImages");
    }

    private static void ValidatePrompt(string prompt, string requestTypeName)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException($"{requestTypeName}.Prompt is required.", nameof(prompt));
        }
    }

    private static void ValidateSeed(int? seed, string requestTypeName)
    {
        if (seed is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seed), $"{requestTypeName}.Seed must be between 0 and {int.MaxValue}.");
        }
    }

    private static void ValidateNumImages(int? numImages, string requestTypeName)
    {
        if (numImages is < 1 or > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(numImages), $"{requestTypeName}.NumImages must be between 1 and 8.");
        }
    }

    private static void ValidateRenderingSpeed(string? renderingSpeed, string requestTypeName, bool allowFlash = true)
    {
        if (string.IsNullOrWhiteSpace(renderingSpeed))
        {
            return;
        }

        if (!IdeogramRenderingSpeed.All.Contains(renderingSpeed))
        {
            throw new ArgumentException($"{requestTypeName}.RenderingSpeed must be one of the known Ideogram rendering speeds.", nameof(renderingSpeed));
        }

        if (!allowFlash && string.Equals(renderingSpeed, IdeogramRenderingSpeed.Flash, StringComparison.Ordinal))
        {
            throw new ArgumentException($"{requestTypeName}.RenderingSpeed cannot be FLASH.", nameof(renderingSpeed));
        }
    }

    private static void ValidateMagicPrompt(string? magicPrompt, string requestTypeName)
    {
        if (string.IsNullOrWhiteSpace(magicPrompt))
        {
            return;
        }

        if (!IdeogramMagicPrompt.All.Contains(magicPrompt))
        {
            throw new ArgumentException($"{requestTypeName}.MagicPrompt must be AUTO, ON, or OFF.", nameof(magicPrompt));
        }
    }

    private static void ValidateResolutionAndAspectRatio(string? resolution, string? aspectRatio, string requestTypeName)
    {
        if (!string.IsNullOrWhiteSpace(resolution) && !string.IsNullOrWhiteSpace(aspectRatio))
        {
            throw new ArgumentException(
                $"{requestTypeName}.Resolution and {requestTypeName}.AspectRatio cannot both be set.");
        }

        ValidateOptionalResolution(resolution, requestTypeName);
        ValidateAspectRatio(aspectRatio, requestTypeName);
    }

    private static void ValidateRequiredResolution(string resolution, string requestTypeName)
    {
        if (string.IsNullOrWhiteSpace(resolution))
        {
            throw new ArgumentException($"{requestTypeName}.Resolution is required.", nameof(resolution));
        }

        ValidateOptionalResolution(resolution, requestTypeName);
    }

    private static void ValidateOptionalResolution(string? resolution, string requestTypeName)
    {
        if (string.IsNullOrWhiteSpace(resolution))
        {
            return;
        }

        if (!IdeogramResolutions.All.Contains(resolution))
        {
            throw new ArgumentException($"{requestTypeName}.Resolution must be one of the known Ideogram v3 resolutions.", nameof(resolution));
        }
    }

    private static void ValidateAspectRatio(string? aspectRatio, string requestTypeName)
    {
        if (string.IsNullOrWhiteSpace(aspectRatio))
        {
            return;
        }

        var normalized = IdeogramAspectRatios.Normalize(aspectRatio);
        if (!IdeogramAspectRatios.All.Contains(normalized))
        {
            throw new ArgumentException($"{requestTypeName}.AspectRatio must be one of the known Ideogram aspect ratios.", nameof(aspectRatio));
        }
    }

    private static void ValidateStyleType(string? styleType, string requestTypeName)
    {
        if (string.IsNullOrWhiteSpace(styleType))
        {
            return;
        }

        if (!IdeogramStyleTypes.All.Contains(styleType))
        {
            throw new ArgumentException($"{requestTypeName}.StyleType must be one of the known Ideogram style types.", nameof(styleType));
        }
    }

    private static void ValidateStylePreset(string? stylePreset, string requestTypeName)
    {
        if (string.IsNullOrWhiteSpace(stylePreset))
        {
            return;
        }

        if (!IdeogramStylePresets.All.Contains(stylePreset))
        {
            throw new ArgumentException($"{requestTypeName}.StylePreset must be one of the known Ideogram style presets.", nameof(stylePreset));
        }
    }

    private static void ValidateStyleCodes(
        IReadOnlyList<string>? styleCodes,
        IReadOnlyList<IdeogramFile>? styleReferenceImages,
        string? styleType,
        string requestTypeName)
    {
        if (styleCodes is null)
        {
            return;
        }

        foreach (var styleCode in styleCodes)
        {
            if (string.IsNullOrWhiteSpace(styleCode) || !StyleCodeRegex().IsMatch(styleCode))
            {
                throw new ArgumentException($"{requestTypeName}.StyleCodes entries must match ^[0-9A-Fa-f]{{8}}$.", nameof(styleCodes));
            }
        }

        if (styleReferenceImages is { Count: > 0 })
        {
            throw new InvalidOperationException($"{requestTypeName}.StyleCodes cannot be used with {requestTypeName}.StyleReferenceImages.");
        }

        if (!string.IsNullOrWhiteSpace(styleType))
        {
            throw new InvalidOperationException($"{requestTypeName}.StyleCodes cannot be used with {requestTypeName}.StyleType.");
        }
    }

    private static void ValidateImageWeight(int? imageWeight)
    {
        if (imageWeight is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(imageWeight), "RemixRequest.ImageWeight must be between 1 and 100.");
        }
    }

    private static void ValidateRequiredFile(IdeogramFile file, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(file);
        ValidateFile(file, propertyName);

        if (file.Length is > MaxImageBytes)
        {
            throw new ArgumentOutOfRangeException(propertyName, $"{propertyName} must be <= 10000000 bytes.");
        }
    }

    private static void ValidateFilesTotal(IReadOnlyList<IdeogramFile>? files, string propertyName)
    {
        if (files is null)
        {
            return;
        }

        long totalLength = 0;
        foreach (var file in files)
        {
            ArgumentNullException.ThrowIfNull(file);
            ValidateFile(file, propertyName);
            totalLength += file.Length ?? 0;
        }

        if (totalLength > MaxImageBytes)
        {
            throw new ArgumentOutOfRangeException(propertyName, $"{propertyName} total size must be <= 10000000 bytes.");
        }
    }

    private static void ValidateFile(IdeogramFile file, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(file.FileName))
        {
            throw new ArgumentException($"{propertyName}.FileName is required.", propertyName);
        }

        if (!MimeTypeDetector.IsSupportedContentType(file.ContentType))
        {
            throw new ArgumentException($"{propertyName}.ContentType must be a supported image MIME type.", propertyName);
        }

        if (Path.HasExtension(file.FileName) && !MimeTypeDetector.IsSupportedFileExtension(file.FileName))
        {
            throw new ArgumentException($"{propertyName}.FileName must use a supported image extension.", propertyName);
        }

        if (file.Length is null or < 0)
        {
            throw new ArgumentException($"{propertyName}.Length must be known and non-negative.", propertyName);
        }
    }

    private static void ValidateCharacterMaskPairing(
        IReadOnlyList<IdeogramFile>? characterReferenceImages,
        IReadOnlyList<IdeogramFile>? characterReferenceImageMasks,
        string requestTypeName)
    {
        if (characterReferenceImageMasks is null)
        {
            return;
        }

        if (characterReferenceImages is null)
        {
            throw new InvalidOperationException(
                $"{requestTypeName}.CharacterReferenceImageMasks requires {requestTypeName}.CharacterReferenceImages.");
        }

        if (characterReferenceImageMasks.Count != characterReferenceImages.Count)
        {
            throw new InvalidOperationException(
                $"{requestTypeName}.CharacterReferenceImageMasks count must match {requestTypeName}.CharacterReferenceImages count.");
        }
    }

    private static void ValidateColorPalette(ColorPalette? colorPalette)
    {
        if (colorPalette is null)
        {
            return;
        }

        var hasName = !string.IsNullOrWhiteSpace(colorPalette.Name);
        var hasMembers = colorPalette.Members is { Count: > 0 };

        if (hasName == hasMembers)
        {
            throw new InvalidOperationException("ColorPalette must specify either Name or Members, not both.");
        }

        if (hasName && !IdeogramColorPalettes.All.Contains(colorPalette.Name!))
        {
            throw new ArgumentException("ColorPalette.Name must be one of the known palette constants.", nameof(colorPalette));
        }

        if (!hasMembers)
        {
            return;
        }

        if (colorPalette.Members!.Count is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(colorPalette), "ColorPalette.Members must contain between 1 and 5 entries.");
        }

        foreach (var member in colorPalette.Members)
        {
            ArgumentNullException.ThrowIfNull(member);

            if (string.IsNullOrWhiteSpace(member.ColorHex) || !ColorHexRegex().IsMatch(member.ColorHex))
            {
                throw new ArgumentException("ColorPaletteMember.ColorHex must match ^#[0-9A-Fa-f]{6}$.", nameof(colorPalette));
            }

            if (member.ColorWeight is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(colorPalette), "ColorPaletteMember.ColorWeight must be between 0 and 1.");
            }
        }
    }

    private static void ValidateUpscaleFactor(string? upscaleFactor)
    {
        if (string.IsNullOrWhiteSpace(upscaleFactor))
        {
            return;
        }

        if (!IdeogramUpscaleFactors.All.Contains(upscaleFactor))
        {
            throw new ArgumentException("GenerateTransparentRequest.UpscaleFactor must be one of X1, X2, or X4.", nameof(upscaleFactor));
        }
    }

    [GeneratedRegex("^[0-9A-Fa-f]{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex StyleCodeRegex();

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex ColorHexRegex();
}
