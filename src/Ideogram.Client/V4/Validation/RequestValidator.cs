using A2G.Ideogram.Client.Constants;
using A2G.Ideogram.Client.Constants.V4;
using A2G.Ideogram.Client.V4.Models;

namespace A2G.Ideogram.Client.V4.Validation;

internal static class RequestValidator
{
    public static void Validate(GenerateFromTextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateTextPrompt(request.TextPrompt, nameof(GenerateFromTextRequest));
        ValidateResolution(request.Resolution, nameof(GenerateFromTextRequest));
        ValidateRenderingSpeed(request.RenderingSpeed, nameof(GenerateFromTextRequest));
    }

    public static void Validate(GenerateFromJsonRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.JsonPrompt);
        ValidateResolution(request.Resolution, nameof(GenerateFromJsonRequest));
        ValidateRenderingSpeed(request.RenderingSpeed, nameof(GenerateFromJsonRequest));
    }

    public static void Validate(RemixRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Image);
        ValidateTextPrompt(request.TextPrompt, nameof(RemixRequest));

        if (request.ImageWeight is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(request.ImageWeight), "RemixRequest.ImageWeight must be between 1 and 100.");
        }

        ValidateResolution(request.Resolution, nameof(RemixRequest));
        ValidateRenderingSpeed(request.RenderingSpeed, nameof(RemixRequest));
    }

    public static void Validate(MagicPromptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateTextPrompt(request.TextPrompt, nameof(MagicPromptRequest));

        if (!string.IsNullOrWhiteSpace(request.AspectRatio) &&
            !AspectRatiosV4.IsValid(request.AspectRatio))
        {
            throw new ArgumentException($"{nameof(MagicPromptRequest)}.AspectRatio must be AUTO or an NxM ratio.", nameof(request.AspectRatio));
        }
    }

    public static void Validate(DescribeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ImageFile);
    }

    private static void ValidateTextPrompt(string value, string requestTypeName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{requestTypeName}.TextPrompt is required.", nameof(value));
        }
    }

    private static void ValidateResolution(string? value, string requestTypeName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!Resolutions.IsValid(value))
        {
            throw new ArgumentException($"{requestTypeName}.Resolution must use WIDTHxHEIGHT format.", nameof(value));
        }
    }

    private static void ValidateRenderingSpeed(string? value, string requestTypeName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!IdeogramRenderingSpeed.All.Contains(value))
        {
            throw new ArgumentException($"{requestTypeName}.RenderingSpeed must be one of the known Ideogram rendering speeds.", nameof(value));
        }
    }
}
