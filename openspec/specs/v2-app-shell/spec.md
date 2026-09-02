## Purpose

Defines the v2 WPF application shell: native Fluent theming, Settings-like left navigation, page hosting, and a stable `Ra3MapUtils.exe` entry identity.

## Requirements

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
The main window MUST present a left navigation pane and a right content area. Selecting a navigation item MUST show the corresponding page in the content area. Navigation MUST include a maps item, a nano-programs item, a toolbox item, and a settings item. The settings item MUST be pinned at the bottom of the left navigation rail, separate from the primary feature list above it.

#### Scenario: User opens map management from navigation
- **WHEN** the user selects the maps navigation item
- **THEN** the right content area MUST display the map management page

#### Scenario: User opens nano programs from navigation
- **WHEN** the user selects the nano-programs navigation item
- **THEN** the right content area MUST display the nano-programs page

#### Scenario: User opens toolbox from navigation
- **WHEN** the user selects the toolbox navigation item
- **THEN** the right content area MUST display the toolbox page

#### Scenario: User opens settings from bottom-pinned navigation
- **WHEN** the user selects the settings navigation item pinned at the bottom of the left rail
- **THEN** the right content area MUST display the settings page

#### Scenario: Initial navigation selection
- **WHEN** the main window finishes loading
- **THEN** the maps navigation item MUST be selected and the map management page MUST be visible

### Requirement: Shell remains usable without other feature pages
Navigation MAY omit legacy feature pages that are not yet migrated. The shell MUST remain fully usable with the maps, nano-programs, toolbox, and settings entries present.

#### Scenario: Maps, nano programs, toolbox, and settings available
- **WHEN** the user views the left navigation
- **THEN** the maps, nano-programs, toolbox, and settings entries MUST be present and selectable
- **AND** the settings entry MUST appear pinned at the bottom of the rail
- **AND** absence of other legacy pages MUST NOT prevent launching or navigating those four entries

### Requirement: Main window close hides to tray
The main window close affordance MUST hide the window and keep the shell process alive rather than shutting down the application. Process exit remains available through the system tray Exit action defined by the system-tray capability.

#### Scenario: Close button does not exit the process
- **WHEN** the user closes the main window via the window close button
- **THEN** the main window MUST hide
- **AND** the application process MUST continue running
