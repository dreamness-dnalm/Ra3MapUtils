## Why

v2 already has map management and a partial Lua import pipeline, but the Lua library manager UI and redundancy-factor setting are missing. The current util/API path still depends on legacy subprojects (`SharedFunctionLib`, `UtilCoreLib`) and `Ra3MapUtils.db`, which conflicts with the v2 direction of owning behavior in `src/Core` + `src/UI` and `Ra3MapUtils.v2.db`.

## What Changes

- Add a v2 Lua import manager window (library list CRUD/reorder, import into the selected map) opened from the maps page context menu
- Persist per-map Lua library configs, Active map pointers (for `IMPORT_LUA_WITH_ACTIVE_CONFIG`), and the Lua redundancy factor in `Ra3MapUtils.v2.db` only
- Rewrite the Lua import pipeline so it does **not** use any legacy subprojects (`SharedFunctionLib`, `UtilCoreLib`, `WBLegacy`, or the legacy `Ra3MapUtils` WPF project) — including config/Active/redundancy stores and map path helpers previously taken from `UtilLib.*`
- Expose the redundancy factor on the settings page; import execution MUST read that v2-backed value
- Keep official nano programs `IMPORT_LUA` / `IMPORT_LUA_WITH_ACTIVE_CONFIG` entrypoints; preserve Active-map semantics for the latter by writing Active state when the manager opens a map
- Remove `SharedFunctionLib` and `UtilCoreLib` `ProjectReference`s from `src/UI/UI.csproj` by also replacing remaining non-Lua call sites that currently force those references (`App` Libs path helper, MCP map folder/file helpers)
- Out of scope: NewWorldBuilder plugin `RA3MapUtil_LuaImporter`; LuaExecutor / ChatLuaHelper; automatic migration of legacy Lua bindings from `Ra3MapUtils.db`

## Capabilities

### New Capabilities

- `lua-import`: v2 Lua library manager, import execution, and v2-backed config/Active-map storage with no legacy subproject dependencies

### Modified Capabilities

- `app-settings`: add Lua redundancy-factor preference on the settings page (v2 DB only)
- `map-file-actions`: enable the Lua import context-menu action to open the manager (no longer a disabled placeholder)

## Impact

- `src/Core`: Lua-import stores + any path helpers needed so UI need not call `UtilCoreLib`
- `src/UI`: Lua manager; SettingPage redundancy control; MapManage context menu; rewrite `MapLuaImporterUtil` / `LuaImportService`; replace `Ra3MapUtilsPathUtil` and MCP `MapFileHelper` usages; drop legacy project references from `UI.csproj`
- Official nano programs keep calling `MapLuaImporterUtil` (no required Main.cs change if public methods stay)
- HTTP/MCP scheme import continues against the new config source
- Users must re-create Lua library bindings in v2 (no legacy DB import in this change)
