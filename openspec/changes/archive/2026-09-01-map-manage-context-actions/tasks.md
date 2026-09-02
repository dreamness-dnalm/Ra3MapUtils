## 1. Core map file service

- [x] 1.1 Add `IMapFileService` (+ result types) for open-path helpers, rename, clone, compress, delete, and map display-name get/set
- [x] 1.2 Implement path safety: only mutate directories that validate as maps; prefer staying under the maps root
- [x] 1.3 Implement rename / clone / delete with clear errors when target exists, missing, or IO fails
- [x] 1.4 Implement zip compress for a single map (fail if output zip already exists)
- [x] 1.5 Implement minimal map.str display-name read/write compatible with common legacy cases
- [x] 1.6 Register the file service in UI DI

## 2. Selection, hint, and page chrome

- [x] 2.1 Enable multi-select on the map list and track selected maps in the ViewModel
- [x] 2.2 Add dismissible session-only hint (close hides for current process only)
- [x] 2.3 Ensure preview stays display-only; for multi-select show a simple selection summary instead of single-map preview details
- [x] 2.4 Keep/add page-level open-maps-root control alongside search and refresh

## 3. Context menu actions

- [x] 3.1 Add context menu with single-map actions: open folder, rename, clone, compress, edit display name, delete
- [x] 3.2 Wire CanExecute so non-delete file actions require exactly one selected map
- [x] 3.3 Implement delete with confirmation for single and batch; refresh list; summarize batch failures
- [x] 3.4 Add disabled placeholder items for Lua, border, terrain, and data migration
- [x] 3.5 After successful mutate actions, refresh catalog and selection sanely

## 4. Verification

- [x] 4.1 Build `src\UI\UI.csproj` successfully
- [x] 4.2 Manually verify: hint dismiss, single-map menu ops, multi-select batch delete confirm/cancel, tool items disabled, preview has no action buttons
