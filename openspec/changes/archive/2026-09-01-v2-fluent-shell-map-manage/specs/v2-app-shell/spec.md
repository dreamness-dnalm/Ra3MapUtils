## Purpose

Defines the v2 WPF application shell: native Fluent theming, Settings-like left navigation, page hosting, and a stable `Ra3MapUtils.exe` entry identity.

## ADDED Requirements

### Requirement: Application entry produces Ra3MapUtils.exe
The v2 UI entry project MUST build a Windows executable named `Ra3MapUtils.exe` so packaging and user-facing identity remain compatible with the pre-refactor product.

#### Scenario: Debug or Release build output name
- **WHEN** the v2 UI project is built
- **THEN** the primary output assembly file name MUST be `Ra3MapUtils.exe`

### Requirement: Native Fluent theme without third-party UI chrome libraries
The application MUST enable the platform WPF Fluent theme (light, dark, or system-following) and MUST NOT depend on WPF-UI or HandyControl for the shell chrome in this capability.

#### Scenario: Application starts with Fluent styling
- **WHEN** the user launches the v2 application
- **THEN** standard WPF controls in the shell MUST use Fluent theme styling consistent with the configured theme mode

### Requirement: Settings-like left navigation hosts pages
The main window MUST present a left navigation pane and a right content area. Selecting a navigation item MUST show the corresponding page in the content area. The first version MUST include a navigation item for map management labeled for maps (地图).

#### Scenario: User opens map management from navigation
- **WHEN** the user selects the maps navigation item
- **THEN** the right content area MUST display the map management page

#### Scenario: Initial navigation selection
- **WHEN** the main window finishes loading
- **THEN** the maps navigation item MUST be selected and the map management page MUST be visible

### Requirement: Shell remains usable without other feature pages
Navigation MAY omit non-map feature pages in the first version. The shell MUST remain fully usable with only the maps entry present.

#### Scenario: Only maps entry available
- **WHEN** the user views the left navigation
- **THEN** at least the maps entry MUST be present and selectable
- **AND** absence of other legacy pages MUST NOT prevent launching or navigating to maps
