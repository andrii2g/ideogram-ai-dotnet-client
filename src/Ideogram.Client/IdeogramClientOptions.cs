namespace Ideogram.Client;

public sealed class IdeogramClientOptions
{
    public required string ApiKey { get; init; }

    public Uri BaseUri { get; init; } = new("https://api.ideogram.ai");

    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromMinutes(3);

    public TimeSpan DownloadTimeout { get; init; } = TimeSpan.FromMinutes(2);

    public string UserAgent { get; init; } =
        "Ideogram.Client/1.0 (+https://github.com/andrii2g/ideogram-ai-dotnet-client)";
}
