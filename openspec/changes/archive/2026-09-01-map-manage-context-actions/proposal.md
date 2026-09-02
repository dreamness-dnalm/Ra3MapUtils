## Why

The v2 map page can list and preview maps but still cannot manage them. Users need the familiar file operations from the legacy app, without cluttering the Settings-like UI with a dense toolbar. Context-menu actions plus multi-select delete keep the surface clean while restoring essential workflows.

## What Changes

- Add map file actions via **list context menu**: open folder, rename, clone (另存为), compress, edit in-game display name (`map.str`), delete.
- Support **multi-select**; the only batch action is **delete selected** (with confirmation and failure summary).
- Keep the preview pane **display-only** (no action buttons).
- Show a **dismissible hint** explaining right-click and multi-select batch delete; dismissal is **session-only** (not persisted).
- Reserve **disabled** context-menu entries for tool workflows (Lua / border / terrain / data migration) without implementing them.
- Keep page-level controls: search, refresh, open maps root folder.
- **BREAKING** (spec): lift the v1 “read-only map catalog” constraint so mutating file operations are allowed.

## Capabilities

### New Capabilities

- `map-file-actions`: Context-menu map operations, multi-select batch delete, dismissible hint, disabled tool placeholders.

### Modified Capabilities

- `map-catalog`: Remove the first-version read-only requirement so map directories may be mutated by file actions.

## Impact

- **Code**: `src/UI` map page / ViewModels (selection, context menu, hint); `src/Core` map file operations (delete/rename/clone/compress, path safety) aligned with legacy `MapFileHelper` rules where practical.
- **Out of scope**: Migrating Lua / border / terrain / data-migration windows; persisting hint dismissal; batch compress or other batch ops.
- **Legacy**: Root `Ra3MapUtils` projects unchanged; behavior parity is selective for file ops only.
