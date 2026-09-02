## MODIFIED Requirements

### Requirement: Tool entries are visible but inactive
The context menu MUST include an actionable Lua import entry that opens the v2 Lua import manager for the current single-map selection. The context menu MUST also include placeholder entries for border management, terrain transform, and data migration; those remaining placeholders MUST be disabled or otherwise non-functional and MUST NOT open tool windows in this change.

#### Scenario: Lua import opens the manager
- **WHEN** the user activates Lua import from the map list context menu with a single map selected
- **THEN** the v2 Lua import manager MUST open for that map

#### Scenario: Other tool placeholders stay inactive
- **WHEN** the user opens the map list context menu
- **THEN** the border management, terrain transform, and data migration placeholder entries MUST be visible
- **AND** activating them MUST NOT launch those tool workflows
