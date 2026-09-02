## Purpose

Provides the v2 toolbox as a categorized launcher of statically registered built-in tools, with a Fluent page for browsing and opening those tools.

## Requirements

### Requirement: Static catalog of toolbox tools
The application MUST maintain a code-registered catalog of toolbox tools. Each entry MUST expose at least an id, display title, short description, category, and launch behavior. Discovery of user-authored packages is out of scope for this capability.

#### Scenario: Catalog lists registered tools
- **WHEN** the toolbox page loads
- **THEN** it MUST show every statically registered tool entry that is included in this change's migration set

#### Scenario: Tools are grouped by category
- **WHEN** the user views the toolbox page
- **THEN** tools MUST appear under category headings rather than a single undifferentiated wall of entries

### Requirement: User can open a registered tool
Selecting an available toolbox entry MUST launch that tool according to its registered launch behavior (for example open a tool window). Unavailable entries MUST NOT launch and MUST indicate why when practical.

#### Scenario: Open an available tool
- **WHEN** the user activates an available toolbox entry
- **THEN** the system MUST perform that entry's launch behavior

#### Scenario: Unavailable tool is not launched
- **WHEN** the user activates a toolbox entry marked unavailable
- **THEN** the system MUST NOT launch the tool
- **AND** MUST present a clear unavailable reason when one is available

### Requirement: Initial migration includes simple tools
This change MUST migrate at least two low-dependency legacy toolbox tools into the v2 catalog so the launcher is demonstrably usable (FastHash calculator and image encoding tool are the default targets).

#### Scenario: FastHash tool is reachable
- **WHEN** the user opens the toolbox page
- **THEN** a FastHash calculator entry MUST be present and openable

#### Scenario: Image encoding tool is reachable
- **WHEN** the user opens the toolbox page
- **THEN** an image encoding tool entry MUST be present and openable

### Requirement: Developer hosting entry is reachable
The toolbox catalog MUST include a developer-hosting entry that opens the developer hosting window when activated.

#### Scenario: Developer hosting tool is present
- **WHEN** the user opens the toolbox page
- **THEN** a developer hosting (local MCP/HTTP) entry MUST be present and openable

#### Scenario: Activating developer hosting opens the window
- **WHEN** the user activates the developer hosting toolbox entry
- **THEN** the system MUST open the developer hosting window

### Requirement: Game debugger toolbox entries are reachable
The toolbox catalog MUST include separate entries for opening the game debugger, resetting the game map path, time control, and the Lua executor. Activating an available entry MUST open or focus the corresponding tool experience from the game-debugger capability.

#### Scenario: Open debugger entry is present
- **WHEN** the user opens the toolbox page
- **THEN** an open-debugger entry MUST be present and openable when its binary is available

#### Scenario: Map path entry is present
- **WHEN** the user opens the toolbox page
- **THEN** a reset-game-map-path entry MUST be present and openable

#### Scenario: Time control entry is present
- **WHEN** the user opens the toolbox page
- **THEN** a time-control entry MUST be present and openable

#### Scenario: Lua executor entry is present
- **WHEN** the user opens the toolbox page
- **THEN** a Lua executor entry MUST be present and openable
