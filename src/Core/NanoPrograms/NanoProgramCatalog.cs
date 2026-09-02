using Core.Paths;

namespace Core.NanoPrograms;

/// <summary>
/// Discovers Official + User packages and merges metadata from the v2 store.
/// </summary>
public sealed class NanoProgramCatalog
{
    private readonly INanoProgramMetaStore _metaStore;

    public NanoProgramCatalog(INanoProgramMetaStore metaStore)
    {
        _metaStore = metaStore;
    }

    public IReadOnlyList<NanoProgramModel> GetNanoPrograms()
    {
        AppDataPaths.EnsureUserDataLayout();

        var roots = new (NanoProgramInstallType Type, string Path)[]
        {
            (NanoProgramInstallType.Official, AppDataPaths.OfficialNanoProgramsPath),
            (NanoProgramInstallType.User, AppDataPaths.UserNanoProgramsPath),
        };

        var metaById = _metaStore.GetAll().ToDictionary(m => m.Id, StringComparer.OrdinalIgnoreCase);
        var usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<NanoProgramModel>();

        foreach (var (type, root) in roots)
        {
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
                continue;
            }

            foreach (var dir in Directory.GetDirectories(root))
            {
                var info = NanoProgramInfoModel.Of(dir, type);
                if (info == null)
                {
                    continue;
                }

                if (!usedIds.Add(info.ID))
                {
                    continue;
                }

                var isEnabled = true;
                var isWbVisible = true;
                var order = -1;
                if (metaById.TryGetValue(info.ID, out var record))
                {
                    isEnabled = record.IsEnabled;
                    isWbVisible = record.IsWbVisible;
                    order = record.OrderNum;
                }

                _metaStore.AddOrUpdate(info.ID, isEnabled, isWbVisible, order);

                result.Add(new NanoProgramModel
                {
                    Info = info,
                    IsEnabled = isEnabled,
                    IsWbVisible = isWbVisible,
                    Order = order,
                });
            }
        }

        _metaStore.DeleteUnused(usedIds);
        return result.OrderBy(p => p.Order).ThenBy(p => p.Info.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }
}
