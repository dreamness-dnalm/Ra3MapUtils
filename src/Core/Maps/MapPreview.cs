namespace Core.Maps;

public sealed class MapPreview
{
    public required string Name { get; init; }

    public required string DirectoryPath { get; init; }

    public required IReadOnlyList<string> Files { get; init; }

    public string? ThumbnailPath { get; init; }

    public DateTime? LastWriteTimeUtc { get; init; }

    public long TotalSizeBytes { get; init; }

    public string SizeDisplay { get; init; } = "";
}
