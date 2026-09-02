## Context

See `proposal.md` for motivation. Constraints that shape the approach:

- Branch `v2` already has a blank `src/UI` (`net10.0-windows`) wired into `Ra3MapUtils.sln`.
- Legacy app uses WPF-UI + HandyControl; v2 shell intentionally does not.
- Legacy map listing lives in `UtilCoreLib` (`MapFileHelper.IsMap` / `Ls`) on net45 and depends on `PathUtil.RA3MapFolder`.
- End-state goal is to delete root legacy projects; this change only seeds `src/UI` + `src/Core`.

## Goals / Non-Goals

**Goals:**

- Split presentation (`src/UI`) from catalog logic (`src/Core`).
- Native Fluent shell with a hand-built Settings-like left nav (no `NavigationView` control from a third-party library).
- Real map enumeration with the same validity rule as legacy (`{name}/{name}.map`).
- DI + CommunityToolkit.Mvvm composition that can grow as more pages land.

**Non-Goals:**

- Deleting or unhooking legacy projects from the solution.
- Porting map mutate operations (copy/rename/delete/compress/Lua/border/etc.).
- Switching `dev_tools/package.ps1` to pack the new exe yet.
- Full WinUI Settings parity (search box, breadcrumbs, ToggleSwitch, etc.).

## Decisions

### 1. Project layout: `UI` + `Core` only

- **Choice**: `src/UI` (WinExe, WPF, ViewModels) and `src/Core` (class library, no WPF).
- **Why**: Enough separation for real disk IO without inventing Domain/Infrastructure projects for one use case.
- **Alternatives**: UI-only monolith (rejects earlier split goal); three+ libraries (premature).

### 2. TFM and theming: `net10.0-windows` + native Fluent

- **Choice**: Keep `net10.0-windows`. Enable Fluent via `Application.ThemeMode="System"` and/or merging `PresentationFramework.Fluent` resources. Suppress `WPF0001` if ThemeMode APIs are used from code.
- **Why**: .NET 10 is LTS, expands Fluent coverage, and matches the existing scaffold.
- **Alternatives**: net8 + WPF-UI (rejected for v2 design language); net9 only (shorter support, fewer Fluent fixes).

### 3. Left navigation: custom ListBox/ItemsControl shell

- **Choice**: Build a left rail with selectable items bound to a navigation VM; host pages in a `ContentControl`/`Frame`.
- **Why**: Platform Fluent has no `NavigationView`; custom rail can still look Settings-like.
- **Alternatives**: WPF-UI `NavigationView` (rejected dependency); WinUI 3 rewrite (out of scope).

### 4. Map catalog implementation location

- **Choice**: Implement enumeration in `src/Core` (preferred). Optionally copy the `IsMap`/`Ls` rules rather than ProjectReference `UtilCoreLib`, to avoid pulling net45/`MapCoreLib` into the new stack. Default maps root: same semantic as legacy `PathUtil.RA3MapFolder` (Documents RA3 maps folder); allow overriding later.
- **Why**: Clean break from net45; behavior compatibility without dragging legacy dependency graph.
- **Alternatives**: ProjectReference UtilCoreLib (faster, dirtier); call into running legacy app (no).

### 5. MVVM and DI

- **Choice**: CommunityToolkit.Mvvm in `UI` only; Microsoft.Extensions.DependencyInjection for service registration in `App` startup (replace pure `StartupUri` with manual MainWindow resolve if needed).
- **Why**: Matches legacy muscle memory; keeps Core free of MVVM attributes.
- **Alternatives**: Hand-rolled INPC; Prism (heavier).

### 6. Exe identity

- **Choice**: `<AssemblyName>Ra3MapUtils</AssemblyName>` (and aligned product title as needed) on `src/UI`.
- **Why**: Packaging and user expectations require `Ra3MapUtils.exe`.

## Risks / Trade-offs

- [Fluent theme still experimental / ThemeMode WPF0001] → Prefer XAML ThemeMode or documented dictionary merge; pin net10; document suppressions.
- [Custom nav looks less “Settings” than WPF-UI] → Invest in spacing, selection brush, typography using Fluent resources; iterate visually.
- [Maps root path divergence from legacy settings DB] → First version uses default Documents maps folder; document as known gap; later change can read shared settings.
- [Dual apps in one solution confuse “which exe to run”] → Document that v2 work runs `src/UI`; packaging still legacy until cutover.
- [Reimplementing IsMap drifts from UtilCoreLib] → Keep the rule identical and add a short comment referencing legacy behavior.

## Migration Plan

1. Flesh out `src/UI` + add `src/Core`; ensure solution builds both.
2. Develop and run v2 shell side-by-side with legacy `Ra3MapUtils` project.
3. Do **not** change Velopack/`package.ps1` mainExe in this change.
4. Later milestone: point packaging at `src/UI`, then remove legacy projects.

Rollback: leave legacy projects untouched; remove or ignore `src/*` if abandoned.

## Open Questions

- Exact default maps root resolution on machines with non-default Documents paths (follow legacy `PathUtil` behavior when reimplemented).
- Whether a future settings page should override maps root before other features land.
