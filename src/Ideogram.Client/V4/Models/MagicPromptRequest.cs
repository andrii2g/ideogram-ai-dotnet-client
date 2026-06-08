namespace A2G.Ideogram.Client.V4.Models;

public sealed class MagicPromptRequest
{
    public required string TextPrompt { get; init; }

    public string? AspectRatio { get; init; }
}
