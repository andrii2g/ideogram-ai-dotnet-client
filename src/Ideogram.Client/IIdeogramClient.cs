using A2G.Ideogram.Client.Models;

namespace A2G.Ideogram.Client;

public interface IIdeogramClient
{
    Task<IdeogramResponse> GenerateAsync(
        GenerateRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> GenerateTransparentAsync(
        GenerateTransparentRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> InpaintAsync(
        InpaintRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> RemixAsync(
        RemixRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> ReframeAsync(
        ReframeRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> ReplaceBackgroundAsync(
        ReplaceBackgroundRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> DownloadImagesAsync(
        IdeogramResponse response,
        string outputDirectory,
        string fileNamePrefix = "ideogram",
        CancellationToken cancellationToken = default);

    Task<string> DownloadImageAsync(
        string imageUrl,
        string outputPath,
        CancellationToken cancellationToken = default);
}
