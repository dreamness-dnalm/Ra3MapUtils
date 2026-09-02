## Why

v2 toolbox still lacks the Ra3Hacker-based game debugging tools that map authors rely on (launch injector, map-load path, time control, in-game Lua execution). Migrating this cluster restores parity for runtime debugging without bringing back abandoned NewWorldBuilder linkage.

## What Changes

- Ship the bundled Ra3Hacker toolchain with the v2 UI output (`Injector`, `setting.json`, native deps, SDK reference as needed)
- Add toolbox entries and windows for: open debugger, reset game map path, time control, and Lua executor (separate cards/windows, legacy-shaped UX)
- Read/write debugger map settings in `setting.json`; warn that running injectors must be restarted for map settings to apply
- Connect time control and Lua executor to the Ra3Hacker local API (companion host remains on `30033`; Hacker API stays on its configured port, typically `30034`)
- Localize new toolbox/window strings (zh-CN / en)
- Activate an already-open companion tool window instead of spawning duplicates where practical
- Out of scope: NewWorldBuilder path/plugin features; map log viewer; merging all debugger UI into a single workbench; companion-side process locks for Injector (Injector already self-locks)

## Capabilities

### New Capabilities

- `game-debugger`: Ra3Hacker launch, map-load settings, time control, and Lua executor behaviors in v2

### Modified Capabilities

- `toolbox-catalog`: register the four game-debugger toolbox entries (and category placement)
- `ui-localization`: cover debugger-related toolbox and window strings

## Impact

- `src/UI`: toolbox catalog/launcher, four tool windows + ViewModels, Ra3Hacker data copy rules, SDK reference, icons per toolbox skill
- `src/Core` (optional): shared debugger settings/session helpers if kept UI-agnostic
- Packaging/output size increases by bundling `data/Ra3Hacker/**`
- Does not change companion Mutex `Ra3MapUtils` or HTTP `:30033` hosting
