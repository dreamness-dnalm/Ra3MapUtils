## Why

Ra3MapUtils v2 needs a clean rebuild under `src/` instead of growing the legacy root projects. The first vertical slice should prove the new stack: native .NET 10 Fluent look (Win11 Settings-like shell), UI/logic separation, and a working map catalog that lists real map folders—while keeping the shipped exe name `Ra3MapUtils.exe`.

## What Changes

- Establish `src/UI` as the v2 WPF entry project (`net10.0-windows`), outputting **`Ra3MapUtils.exe`** (`AssemblyName=Ra3MapUtils`).
- Establish `src/Core` for map-catalog logic (no WPF dependency); UI consumes it via interfaces and DI.
- Use the **native WPF Fluent theme** (`ThemeMode` / `PresentationFramework.Fluent`)—**not** WPF-UI—for Win11-aligned chrome.
- Build a Settings-like shell: **left navigation + right content**; first nav target is **地图管理**.
- Implement map management UI that **lists real maps** from the configured RA3 maps folder (reuse/adapt legacy `IsMap` / list semantics as needed).
- Use **CommunityToolkit.Mvvm** for ViewModels and **Microsoft.Extensions.DependencyInjection** for composition.
- Wire `src/*` into `Ra3MapUtils.sln`; keep legacy root projects for now (deletion is a later milestone, not this change).

## Capabilities

### New Capabilities

- `v2-app-shell`: Application entry, Fluent theme, Settings-like left nav, page hosting, exe naming.
- `map-catalog`: Enumerate and present real map directories for the map management page.

### Modified Capabilities

- （无：仓库尚无 `openspec/specs/` 基线能力）

## Impact

- **New code**: `src/UI`, `src/Core`; solution project entries.
- **Existing**: `src/UI` scaffold may be overwritten/extended; legacy `Ra3MapUtils` / `UtilCoreLib` remain until a later cutover; packaging scripts stay pointed at the old app until the entry switch is intentional.
- **Dependencies**: CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection; no WPF-UI / HandyControl for v2 shell.
- **Runtime**: Reads the RA3 maps directory from disk; no destructive map operations in this change.
