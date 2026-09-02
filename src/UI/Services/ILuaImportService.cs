using UI.Models;

namespace UI.Services;

public interface ILuaImportService
{
    LuaImportSchemeOperationResult ExportMapLuaImportScheme(string map, string jsonPath);

    LuaImportSchemeOperationResult ImportLuaBySchemeJson(string map, string jsonPath);
}
