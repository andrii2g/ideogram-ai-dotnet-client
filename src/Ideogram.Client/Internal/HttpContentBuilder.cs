using Ideogram.Client.Models;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ideogram.Client.Internal;

internal static class HttpContentBuilder
{
    public static void AddRequiredString(
        MultipartFormDataContent content,
        string name,
        string value)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        content.Add(new StringContent(value, Encoding.UTF8), name);
    }

    public static void AddOptionalString(
        MultipartFormDataContent content,
        string name,
        string? value)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        content.Add(new StringContent(value, Encoding.UTF8), name);
    }

    public static void AddOptionalInt(
        MultipartFormDataContent content,
        string name,
        int? value)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!value.HasValue)
        {
            return;
        }

        content.Add(
            new StringContent(value.Value.ToString(CultureInfo.InvariantCulture), Encoding.UTF8),
            name);
    }

    public static void AddRepeatedStrings(
        MultipartFormDataContent content,
        string name,
        IReadOnlyList<string>? values)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (values is null)
        {
            return;
        }

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            content.Add(new StringContent(value, Encoding.UTF8), name);
        }
    }

    public static void AddFile(
        MultipartFormDataContent content,
        string name,
        IdeogramFile file)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(file);

        var stream = file.OpenReadStream();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        content.Add(streamContent, name, file.FileName);
    }

    public static void AddFiles(
        MultipartFormDataContent content,
        string name,
        IReadOnlyList<IdeogramFile>? files)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (files is null)
        {
            return;
        }

        foreach (var file in files)
        {
            AddFile(content, name, file);
        }
    }

    public static void AddColorPalette(
        MultipartFormDataContent content,
        ColorPalette? colorPalette)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (colorPalette is null)
        {
            return;
        }

        var json = JsonSerializer.Serialize(colorPalette, JsonDefaults.Compact);
        content.Add(new StringContent(json, Encoding.UTF8), "color_palette");
    }
}
