namespace Ideogram.Client.ConsoleApp;

internal sealed class ManualRunOptions
{
    public string? ApiKey { get; init; }

    public string? CommandName { get; init; }

    public Dictionary<string, string> Arguments { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public bool? Download { get; init; }

    public bool ShowHelp { get; init; }
}
