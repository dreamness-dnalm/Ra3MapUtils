## Context

See proposal.md for motivation. v2 already ships `MapLuaImporterUtil`, scheme HTTP/MCP endpoints, and two official Lua nano programs, but they still depend on legacy subprojects: `SharedFunctionLib` (LuaImporterBusiness, models, `Ra3MapUtilsPathUtil`) and `UtilCoreLib` (`UtilLib.mapFileHelper` / `mapstrFileHelper`). Settings currently only cover UI language in `Ra3MapUtils.v2.db`. Map context menu still treats Lua import as a disabled placeholder.

## Goals / Non-Goals

**Goals:**
- Lua manager window + maps entry + settings redundancy factor
- v2 SQLite ownership of redundancy factor, Active map, per-map lib configs
- Zero runtime/compile dependency on legacy subprojects for this work: no `SharedFunctionLib`, `UtilCoreLib`, `WBLegacy`, or legacy `Ra3MapUtils` project for Lua import or for remaining call sites that currently pin those `ProjectReference`s on `UI.csproj`
- Preserve `IMPORT_LUA` / `IMPORT_LUA_WITH_ACTIVE_CONFIG` call shapes and Active semantics

**Non-Goals:**
- Migrating or shipping `RA3MapUtil_LuaImporter` plugin
- Auto-importing legacy bindings from `Ra3MapUtils.db`
- LuaExecutor / ChatLuaHelper toolbox tools
- Rewriting unrelated legacy solutions themselves (they may remain in the repo unused by v2)

## Decisions

### Decision: No legacy subprojects
Lua import and the UI project references required to build/run it MUST NOT use `SharedFunctionLib`, `UtilCoreLib`, or other legacy app libraries. Replace:
- Business/DAO/settings → Core v2 stores
- `MapFileHelper` / map.str helpers in the import util → Core and/or Dreamness.RA3.Map.* APIs already referenced via `src/lib`
- `Ra3MapUtilsPathUtil` in `App` → `AppDataPaths` (or equivalent Core path helper)
- MCP map folder/file services’ `UtilLib` usage → Core map path/file services already used by the maps page where possible

**Alternatives considered:** keep `UtilCoreLib` only for path translate. Rejected — user forbids any legacy subproject.

### Decision: All Lua-import state in `Ra3MapUtils.v2.db`
Extend the v2 DB with redundancy factor (settings key), per-map lib config table(s), and Active map name/path keys.

### Decision: No automatic legacy migration
Users reconfigure Lua libraries in v2.

### Decision: Keep Util public methods for nanos
Preserve `ImportLua(string)` / `ImportLuaWithActiveConfig(string)`; resolve Core services via app composition root for script hosts.

### Decision: Manager sets Active map on open
Set Active on open; clear on close; Active-config nano loads bindings for Active map while writing the map file it opens (legacy split).

### Decision: Plugin out of scope
No `RA3MapUtil_LuaImporter` work.

## Risks / Trade-offs

- [Users lose prior Lua bindings] → Accept; optional future migrator
- [MCP map helpers reimplementation] → Reuse `Core.Maps` where possible to avoid duplicate path rules
- [RedundancyStr static cache] → Invalidate when factor changes
- [Broader than Lua-only] → Required so `UI.csproj` can drop legacy `ProjectReference`s

## Migration Plan

1. Core stores + path helpers as needed.
2. Rewrite Util / LuaImportService / App Libs path / MCP map helpers off legacy projects.
3. Remove `SharedFunctionLib` and `UtilCoreLib` from `UI.csproj`.
4. Ship manager UI + settings + enable maps menu.
5. Verify build with no legacy project references; nanos unchanged at Main.cs level.

## Open Questions

*(none — product decisions locked)*
