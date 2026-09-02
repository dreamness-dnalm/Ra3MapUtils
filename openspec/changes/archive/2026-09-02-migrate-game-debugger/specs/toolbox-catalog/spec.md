## ADDED Requirements

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
