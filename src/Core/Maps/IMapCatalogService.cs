namespace Core.Maps;

public interface IMapCatalogService
{
    /// <summary>
    /// Default RA3 maps folder (legacy PathUtil / Ra3PathUtil.RA3MapFolder semantics).
    /// </summary>
    string GetDefaultMapsRoot();

    /// <summary>
    /// Lists valid map directories under <paramref name="mapsRoot"/>, or the default root when null/empty.
    /// Never throws for missing/unreadable roots; reports via <see cref="MapCatalogResult.ErrorMessage"/>.
    /// </summary>
    MapCatalogResult ListMaps(string? mapsRoot = null);

    /// <summary>
    /// Loads preview metadata for a map directory. Returns null when the path is missing or unreadable.
    /// </summary>
    MapPreview? GetPreview(string mapDirectoryPath);
}
