## Purpose

Provides the v2 toolbox as a categorized launcher of statically registered built-in tools, with a Fluent page for browsing and opening those tools.

## ADDED Requirements

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
