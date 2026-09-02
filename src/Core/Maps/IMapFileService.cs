namespace Core.Maps;

public interface IMapFileService
{
    string GetDefaultMapsRoot();

    MapOperationResult OpenInExplorer(string path, bool selectPath = false);

    MapOperationResult Rename(string mapDirectoryPath, string newMapName);

    MapOperationResult Clone(string mapDirectoryPath, string newMapName, string? newDisplayName = null);

    MapOperationResult CompressZip(string mapDirectoryPath);

    MapOperationResult Delete(string mapDirectoryPath);

    MapBatchDeleteResult DeleteMany(IEnumerable<string> mapDirectoryPaths);

    string? GetDisplayName(string mapDirectoryPath);

    MapOperationResult SetDisplayName(string mapDirectoryPath, string displayName);
}
