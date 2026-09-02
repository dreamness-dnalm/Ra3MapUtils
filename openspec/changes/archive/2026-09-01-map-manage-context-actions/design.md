## Context

See `proposal.md` for motivation. The v2 shell already lists maps and shows a display preview (`src/UI` + `src/Core`). Legacy mutating ops live in `UtilCoreLib.MapFileHelper` and related helpers; this change reimplements needed file ops in `Core` rather than referencing net45 libraries. UX decision: context menu primary (path B), dismissible session hint, batch delete only, tool placeholders disabled.

## Goals / Non-Goals

**Goals:**

- Wire ListBox multi-select + context menu command surface.
- Implement Core file services for open/rename/clone/compress/delete (+ map.str display name edit) with confirmation for delete.
- Session-only dismissible hint; preview stays display-only.
- Disabled tool menu stubs for future migration.

**Non-Goals:**

- Persisting hint dismissal.
- Implementing Lua / border / terrain / data-migration UIs.
- Batch operations other than delete.
- Preview-pane action buttons.

## Decisions

### 1. Context menu as the only map-targeted action surface

- **Choice**: Put single-map and batch-delete commands on the list `ContextMenu`; keep search/refresh/open-root on the page toolbar.
- **Why**: Cleaner Settings-like layout; matches “file manager” habits for folder ops.
- **Alternatives**: Preview buttons (rejected for clutter); full top toolbar (legacy look).

### 2. Core owns mutating file IO

- **Choice**: Add something like `IMapFileService` in `src/Core` mirroring legacy `IsMap` safety: only operate on directories that validate as maps under the maps root.
- **Why**: Keeps UI free of path logic; avoids ProjectReference to `UtilCoreLib`.
- **Alternatives**: Call legacy helpers via multi-targeting (rejected dependency drag).

### 3. Selection model

- **Choice**: Extended/multiple selection on the map list. Context menu uses current selection; for “open folder” / rename / etc., require single selection. Batch delete uses full selection set.
- **Why**: Spec allows batch delete only; simplifies command `CanExecute`.
- **Alternatives**: Separate “batch mode” toggle (extra UI).

### 4. Delete confirmation and partial failure

- **Choice**: Always confirm (single and batch). On batch, continue after individual failures and show a short summary (count succeeded / failed + first errors).
- **Why**: Destructive; maps may be locked by WorldBuilder.
- **Alternatives**: Stop at first failure (worse UX for large selections).

### 5. Hint dismissal

- **Choice**: `IsHintVisible` in-memory on the page VM; close sets false; no settings/DAO write.
- **Why**: Explicit product choice for this change.
- **Alternatives**: Persist in SQLite (deferred).

### 6. map.str display name

- **Choice**: Minimal Core helper to read/write the map display name consistent with legacy behavior where feasible; prompt via simple input dialog in UI.
- **Why**: Listed as a first-wave file action, not a “tool window”.
- **Alternatives**: Defer to later (would shrink menu unexpectedly vs agreement).

### 7. Compress

- **Choice**: Zip map folder next to / into maps root following legacy compress semantics as closely as practical (zip).
- **Why**: Common share workflow; single-map only.
- **Alternatives**: External 7z only (legacy had zip path).

## Risks / Trade-offs

- [Right-click discoverability] → Session hint; may add persisted hint later.
- [Delete while map open in editor] → Catch IO errors; report failure; do not crash.
- [Clone/rename name collisions] → Validate target does not exist; surface error dialog.
- [map.str format edge cases] → Keep change minimal; fall back to clear error if parse/write fails.
- [Tool placeholders confuse users] → Disabled state + clear labels; no fake success toasts required.

## Migration Plan

1. Ship beside existing list/preview; no package.ps1 cutover.
2. Users on v2 gain context actions; legacy app unchanged.
3. Later changes enable tool menu items by replacing stubs.

Rollback: revert UI menu + Core file service; listing/preview remain.

## Open Questions

- Exact compress output path naming if a zip already exists (overwrite vs suffix) — match legacy when implementing; safe default: fail if exists.
