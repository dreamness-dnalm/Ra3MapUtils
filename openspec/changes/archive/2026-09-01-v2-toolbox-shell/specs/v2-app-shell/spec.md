## MODIFIED Requirements

### Requirement: Settings-like left navigation hosts pages
The main window MUST present a left navigation pane and a right content area. Selecting a navigation item MUST show the corresponding page in the content area. Navigation MUST include a maps item (地图), a nano-programs item (微程序), and a toolbox item (工具箱).

#### Scenario: User opens map management from navigation
- **WHEN** the user selects the maps navigation item
- **THEN** the right content area MUST display the map management page

#### Scenario: User opens nano programs from navigation
- **WHEN** the user selects the nano-programs navigation item
- **THEN** the right content area MUST display the nano-programs page

#### Scenario: User opens toolbox from navigation
- **WHEN** the user selects the toolbox navigation item
- **THEN** the right content area MUST display the toolbox page

#### Scenario: Initial navigation selection
- **WHEN** the main window finishes loading
- **THEN** the maps navigation item MUST be selected and the map management page MUST be visible

### Requirement: Shell remains usable without other feature pages
Navigation MAY omit legacy feature pages that are not yet migrated. The shell MUST remain fully usable with the maps, nano-programs, and toolbox entries present.

#### Scenario: Maps, nano programs, and toolbox available
- **WHEN** the user views the left navigation
- **THEN** the maps, nano-programs, and toolbox entries MUST be present and selectable
- **AND** absence of other legacy pages MUST NOT prevent launching or navigating those three entries
