using A2G.Ideogram.Client.Models;
using A2G.Ideogram.Client.V4.Models;
using V4RemixRequest = A2G.Ideogram.Client.V4.Models.RemixRequest;

namespace A2G.Ideogram.Client;

public interface IIdeogramV4Client
{
    Task<IdeogramResponse> GenerateFromTextAsync(
        GenerateFromTextRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> GenerateFromJsonAsync(
        GenerateFromJsonRequest request,
        CancellationToken cancellationToken = default);

    Task<IdeogramResponse> RemixAsync(
        V4RemixRequest request,
        CancellationToken cancellationToken = default);

    Task<MagicPromptResponse> GenerateMagicPromptAsync(
        MagicPromptRequest request,
        CancellationToken cancellationToken = default);

    Task<DescribeResponse> DescribeAsync(
        DescribeRequest request,
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
