namespace Core.Maps;

public sealed record MapEntry(
    string Name,
    string DirectoryPath,
    int FileCount,
    DateTime? LastWriteTimeUtc,
    string Summary);
