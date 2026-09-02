## Purpose

Provides the v2 Lua import manager and import pipeline: per-map library configs, Active-map state for nano programs, and map script import — all backed by the v2 AppData database with no dependency on legacy subprojects (`SharedFunctionLib`, `UtilCoreLib`, or the legacy WPF app).

## ADDED Requirements

### Requirement: Lua import manager is available for a map
The application MUST provide a Lua import manager window that can be opened for a selected map. The manager MUST allow the user to add, remove, rename, reorder, and set paths for Lua library entries bound to that map, and MUST be able to run an import of the enabled libraries into the map.

#### Scenario: User opens manager from map context
- **WHEN** the user chooses Lua import for a selected map
- **THEN** the Lua import manager MUST open for that map

#### Scenario: User imports libraries into the map
- **WHEN** the user triggers import from the manager with at least one library path configured
- **THEN** the system MUST write the corresponding script groups into the map and save the map

### Requirement: Per-map Lua library configs persist in the v2 database
Lua library binding records (map key, display name, library path, order, and enablement as applicable) MUST be stored in the v2 AppData SQLite database. The system MUST NOT read or write those records through legacy subprojects (`SharedFunctionLib`, `UtilCoreLib`) or the legacy `Ra3MapUtils.db` for this capability.

#### Scenario: Bindings survive restart
- **WHEN** the user configures libraries for a map and later restarts the application
- **THEN** reopening the manager for that map MUST show the same bindings from the v2 database

### Requirement: Active map state supports Import Lua with active config
While the Lua import manager is open for a map, the system MUST record an Active map identity sufficient for `ImportLuaWithActiveConfig` to load that map’s bindings. Closing the manager MAY clear Active state. Active map state MUST live in the v2 database (or equivalent v2 store), not legacy SettingsDAO or SharedFunctionLib.

#### Scenario: Active-config nano uses manager’s map
- **WHEN** the manager is open for map A and the user runs the official “import Lua with active config” nano program with a map file path argument or selection
- **THEN** the import MUST use the library bindings associated with the Active map recorded by the manager (same semantic as legacy Active-map behavior)

### Requirement: Import pipeline does not use legacy subprojects
Map Lua import execution used by the manager, scheme/API import, and official Lua nano programs MUST obtain library lists, Active map, redundancy factor, and map path resolution from v2 Core/UI code and Dreamness map libraries only. It MUST NOT reference or call `SharedFunctionLib`, `UtilCoreLib` (`UtilLib.*`), `WBLegacy`, or the legacy `Ra3MapUtils` WPF project for those operations.

#### Scenario: Import reads v2 redundancy factor
- **WHEN** an import runs after the user saved a redundancy factor in settings
- **THEN** the appended redundancy padding length MUST follow the v2-stored factor

#### Scenario: Import util builds without legacy project references
- **WHEN** the v2 UI project is built after this change
- **THEN** the Lua import pipeline types MUST compile without `ProjectReference` to `SharedFunctionLib` or `UtilCoreLib`

### Requirement: NewWorldBuilder Lua importer plugin is out of scope
This capability MUST NOT migrate, rewrite, or depend on the legacy `RA3MapUtil_LuaImporter` NewWorldBuilder plugin.

#### Scenario: Plugin remains untouched
- **WHEN** this change is implemented
- **THEN** the `RA3MapUtil_LuaImporter` plugin package MUST remain outside the required deliverables of this change
