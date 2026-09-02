namespace Core.Maps;

public sealed class MapCatalogResult
{
    public required string MapsRoot { get; init; }

    public required IReadOnlyList<MapEntry> Maps { get; init; }

    public string? ErrorMessage { get; init; }

    public bool Succeeded => ErrorMessage is null;
}
