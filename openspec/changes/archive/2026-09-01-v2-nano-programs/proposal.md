## Why

v2 already covers map listing and file actions, but micro-programs (微程序) remain only on the legacy app. World Builder and users rely on discoverable scripts plus the local HTTP API. v2 needs a working nano-program stack—not a placeholder page—so the refactor can become the daily driver.

## What Changes

- Add a usable **微程序** page (list, search, enable/visibility, order, run, open folders, run result) under the Settings-like shell.
- Discover Official (`data/nano_programs`) and User (`%AppData%/.../nano_programs/user`) packages (`info.json` + `Main.cs`); execute via ScriptExecutor with legacy-compatible globals (`ArgumentDictionary`, including optional `MapFilePath`).
- Persist enable / WB-visible / order in a **new v2 SQLite database** whose filename MUST NOT be `Ra3MapUtils.db` (e.g. `Ra3MapUtils.v2.db`).
- Provide script dialog surface so packages that prompt for maps/messages still work (WinForms transition allowed for this change).
- Embed local HTTP API on **`127.0.0.1:30033`** with nano-program endpoints compatible with legacy routes (`list`, `wb_visible_list`, `run/{id}`).
- Enforce **single-instance** using the same Mutex name as legacy (`Ra3MapUtils`) so only one companion process owns port 30033.
- Wire Dreamness map/script libraries and CopyToOutput for built-in nano programs into `src/UI`.
- Out of scope: store install UI, MCP-specific nano tools, migrating enable-state from the legacy DB, map-page selection→MapFilePath coupling.

## Capabilities

### New Capabilities

- `nano-programs`: Discovery, metadata, UI management, execution, and run results for micro-programs.
- `v2-local-hosting`: Single-instance process + embedded HTTP API on port 30033 including nano-program routes.

### Modified Capabilities

- `v2-app-shell`: Navigation MUST include 微程序 in addition to 地图; shell is no longer “maps-only”.

## Impact

- **Code**: `src/UI` (page, nav, hosting, dialogs), `src/Core` (nano catalog/meta/execution abstractions + v2 DB access), references to `lib/Dreamness.*` and ScriptExecutor, ASP.NET Core hosting in the UI process.
- **Data**: New AppData DB file; Official packages under output `data/nano_programs`.
- **External**: WB / tools calling `localhost:30033` continue on the same port against the v2 process when it is the running instance.
- **Legacy**: Root projects unchanged this change; dual Mutex prevents legacy and v2 from running together.
