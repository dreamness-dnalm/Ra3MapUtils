## Purpose

Lets users manage map folders from the v2 map page via context menu, including single-map file operations and multi-select batch delete, while keeping the preview pane clean.

## ADDED Requirements

### Requirement: Context menu exposes single-map file actions
When exactly one map is the context target, the map list context menu MUST offer: open folder, rename, clone (另存为), compress, edit in-game display name, and delete. Successful mutating actions MUST refresh the catalog list afterward.

#### Scenario: Open map folder from context menu
- **WHEN** the user chooses open folder on a single selected map
- **THEN** the system MUST open that map's directory in the OS file explorer

#### Scenario: Delete single map with confirmation
- **WHEN** the user chooses delete on a single map and confirms the prompt
- **THEN** the system MUST remove that map directory from disk and remove it from the list
- **AND** if the user cancels the prompt, the map MUST remain unchanged

### Requirement: Multi-select supports batch delete only
The map list MUST allow selecting multiple maps. The only batch mutating action MUST be delete selected. Other file actions MUST apply only when a single map is the effective target.

#### Scenario: Batch delete selected maps
- **WHEN** the user has selected two or more maps and chooses delete selected from the context menu and confirms
- **THEN** the system MUST attempt to delete each selected map directory
- **AND** the UI MUST refresh the list and report any failures without crashing

#### Scenario: Non-delete actions disabled for multi-select
- **WHEN** two or more maps are selected
- **THEN** rename, clone, compress, edit display name, and open-folder-for-one-map MUST NOT run as batch operations

### Requirement: Preview pane remains display-only
The map preview pane MUST NOT host action buttons for file or tool operations. Preview continues to show metadata for the current selection context (single map details, or a multi-select summary).

#### Scenario: No action buttons in preview
- **WHEN** the user views the map management page with a map selected
- **THEN** file and tool actions MUST be reachable from the context menu (and page-level global controls), not from preview action buttons

### Requirement: Dismissible session hint explains discovery
The page MUST show a dismissible hint that explains right-click actions and multi-select batch delete. Dismissing the hint MUST hide it for the current application session only and MUST NOT persist the preference to disk in this change.

#### Scenario: User dismisses the hint
- **WHEN** the user closes the hint
- **THEN** the hint MUST disappear for the rest of the session
- **AND** starting a new application process MUST show the hint again

### Requirement: Tool entries are visible but inactive
The context menu MUST include placeholder entries for Lua import, border management, terrain transform, and data migration. Those entries MUST be disabled or otherwise non-functional and MUST NOT open tool windows in this change.

#### Scenario: Tool placeholder is not actionable
- **WHEN** the user opens the map list context menu
- **THEN** the tool placeholder entries MUST be visible
- **AND** activating them MUST NOT launch legacy tool workflows

### Requirement: Page-level global controls remain available
The map management page MUST continue to provide search, refresh, and open-maps-root actions without requiring a map context selection.

#### Scenario: Open maps root without selection
- **WHEN** the user triggers open maps root from the page
- **THEN** the system MUST open the configured maps root folder in the OS file explorer
