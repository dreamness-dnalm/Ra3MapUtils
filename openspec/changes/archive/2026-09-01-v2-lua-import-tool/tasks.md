## 1. Core v2 persistence (no legacy subprojects)

- [x] 1.1 Add v2 store APIs for Lua redundancy factor (default 100 when unset) in `Ra3MapUtils.v2.db`
- [x] 1.2 Add v2 persistence for per-map Lua library configs (CRUD, reorder, enablement as needed)
- [x] 1.3 Add v2 Active map name/path get/set/clear used by ImportLuaWithActiveConfig
- [x] 1.4 Provide Core (or UI) map path helpers needed by import so `UtilCoreLib` / `UtilLib.*` is not required

## 2. Import pipeline rewrite

- [x] 2.1 Rewrite `MapLuaImporterUtil` to use Core stores + Core/Dreamness path APIs only; invalidate redundancy cache when factor changes
- [x] 2.2 Rewrite `LuaImportService` scheme export/import onto v2 config store (keep HTTP/MCP contracts)
- [x] 2.3 Confirm official nanos `IMPORT_LUA` / `IMPORT_LUA_WITH_ACTIVE_CONFIG` still call existing Util methods without Main.cs changes

## 3. Drop legacy project references from UI

- [x] 3.1 Replace `Ra3MapUtilsPathUtil` usage in `App.xaml.cs` with Core `AppDataPaths` (or equivalent)
- [x] 3.2 Replace MCP `MapFileService` / `MapFolderService` `UtilLib.mapFileHelper` usage with Core map services
- [x] 3.3 Remove `SharedFunctionLib` and `UtilCoreLib` `ProjectReference`s from `src/UI/UI.csproj` and confirm build

## 4. Settings UI

- [x] 4.1 Add Lua redundancy factor control on SettingPage (localized) wired to the v2 store
- [x] 4.2 Verify applying the factor affects the next import without any legacy subproject

## 5. Lua manager UI and map entry

- [x] 5.1 Add Lua import manager window + ViewModel (list CRUD/reorder/path, import action, docs link as appropriate)
- [x] 5.2 On open/close, set/clear Active map in the v2 store
- [x] 5.3 Enable maps context-menu Lua import to open the manager for single selection; leave other tool placeholders disabled
- [x] 5.4 Register DI for the window/services

## 6. Verification

- [x] 6.1 Build `src\UI\UI.csproj` successfully with no ProjectReference to SharedFunctionLib or UtilCoreLib
- [x] 6.2 Verify: settings factor persists in v2 DB; manager bindings persist; import uses v2 factor; Active-config nano follows manager Active map; plugin not touched
