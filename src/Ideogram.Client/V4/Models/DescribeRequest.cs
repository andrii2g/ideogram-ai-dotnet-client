namespace A2G.Ideogram.Client.V4.Models;

public sealed class DescribeRequest
{
    public required IdeogramFile ImageFile { get; init; }

    public bool? IncludeBoundingBoxes { get; init; }
}
