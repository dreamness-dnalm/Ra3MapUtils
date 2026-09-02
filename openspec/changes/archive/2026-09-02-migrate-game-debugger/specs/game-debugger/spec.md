## Purpose

Provides v2 access to the bundled Ra3Hacker game-debugging toolchain: launching the injector, editing map-load settings, controlling in-game time, and executing Lua against a running debug session.

## ADDED Requirements

### Requirement: User can launch the game debugger
The application MUST provide a way to start the bundled Ra3Hacker injector executable shipped with the companion. The companion MUST NOT implement its own process-level single-instance lock for the injector; injector self-locking is authoritative. If the injector binary is missing, the user MUST receive a clear failure indication.

#### Scenario: Launch debugger from toolbox
- **WHEN** the user activates the open-debugger toolbox entry and the injector binary is present
- **THEN** the system MUST start the bundled injector

#### Scenario: Missing injector is reported
- **WHEN** the user activates the open-debugger entry and the injector binary is absent
- **THEN** the system MUST NOT pretend success
- **AND** MUST show a clear error that the debugger is missing

### Requirement: User can edit debugger map-load settings
The application MUST allow editing the debugger map-load folder and the hide-built-in-maps flag persisted in the bundled Ra3Hacker settings file. Saving MUST validate that a non-empty folder path exists. The UI MUST warn that a running debugger must be restarted for map-load changes to take effect.

#### Scenario: Save map folder and hide-built-in flag
- **WHEN** the user sets a valid map folder and hide-built-in preference and saves
- **THEN** those values MUST be written to the debugger settings file

#### Scenario: Invalid map folder rejected
- **WHEN** the user tries to save a non-empty map folder path that does not exist
- **THEN** the system MUST reject the save and leave the previous valid settings intact

### Requirement: Time control talks to the debugger API
While a compatible Ra3Hacker debug session API is reachable, the time-control tool MUST be able to refresh time status and issue pause / fast / single-step style commands exposed by that API. When the API is unreachable, the tool MUST show a clear connectivity failure rather than silently succeeding.

#### Scenario: Refresh time status when API is up
- **WHEN** the debugger API is reachable and the user refreshes time status
- **THEN** the tool MUST display current time/frame-related status from the API

#### Scenario: Time command fails when API is down
- **WHEN** the debugger API is unreachable and the user issues a time-control action
- **THEN** the tool MUST report that the debugger API could not be contacted

### Requirement: Lua executor talks to the debugger API
The Lua executor tool MUST send Lua execution requests through the Ra3Hacker API using the bundled settings. When the API is unreachable, the tool MUST report failure clearly. The tool MUST NOT depend on NewWorldBuilder paths or plugins.

#### Scenario: Execute Lua when API is up
- **WHEN** the debugger API is reachable and the user runs Lua through the executor
- **THEN** the system MUST submit the script via the debugger API and present the API result to the user

#### Scenario: Lua executor fails when API is down
- **WHEN** the debugger API is unreachable and the user attempts to run Lua
- **THEN** the tool MUST report that the debugger API could not be contacted

### Requirement: Companion debugger tool windows are single-instance per tool
Opening a debugger-related companion tool window that is already open MUST activate the existing window instead of creating another instance of that same tool window.

#### Scenario: Reopening map settings activates existing window
- **WHEN** the map-path settings window is already open and the user opens that toolbox entry again
- **THEN** the existing window MUST be activated
- **AND** a second map-path settings window MUST NOT be created

### Requirement: NewWorldBuilder linkage remains out of scope
This capability MUST NOT migrate NewWorldBuilder path binding, plugin installation/management, or the map log viewer that depends on NewWorldBuilder logs.

#### Scenario: No NWB or log-viewer requirement in this capability
- **WHEN** this change is implemented
- **THEN** NewWorldBuilder linkage and map log viewer MUST remain outside the required deliverables of this capability
