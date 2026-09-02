## Purpose

Provides the v2 system tray presence so the companion can stay running after the main window is closed, expose navigation shortcuts from the tray menu, and cooperate with single-instance relaunches.

## ADDED Requirements

### Requirement: System tray icon is present while the app runs
While the application is running as the Mutex holder, the system MUST show a system tray icon for Ra3MapUtils. The tray implementation MUST NOT depend on HandyControl or WPF-UI.

#### Scenario: Tray visible after startup
- **WHEN** the application finishes startup successfully
- **THEN** a system tray icon for the companion MUST be visible

### Requirement: Closing the main window hides to the tray
Closing the main window (for example via the window close button) MUST cancel process shutdown, hide the main window, and keep the process running with the tray icon and local hosting services available. True process exit MUST be available from the tray Exit action.

#### Scenario: User closes the main window
- **WHEN** the user closes the main window while the application is running
- **THEN** the main window MUST hide
- **AND** the process MUST remain running
- **AND** the tray icon MUST remain available

#### Scenario: User exits from the tray
- **WHEN** the user chooses Exit from the tray menu
- **THEN** the application MUST shut down the process (including local HTTP/MCP hosting)

### Requirement: Tray menu exposes show and navigation shortcuts
The tray context menu MUST include actions to show the main window, navigate to Maps, Nano programs, Toolbox, and Settings, and Exit. Choosing a navigation action MUST show and activate the main window and select the corresponding shell navigation target.

#### Scenario: Show main window from tray
- **WHEN** the user chooses Show from the tray menu while the main window is hidden
- **THEN** the main window MUST become visible and activated

#### Scenario: Open Settings from tray
- **WHEN** the user chooses Settings from the tray menu
- **THEN** the main window MUST be shown and activated
- **AND** the settings page MUST be selected in the shell navigation

#### Scenario: Open Maps from tray
- **WHEN** the user chooses Maps from the tray menu
- **THEN** the main window MUST be shown and activated
- **AND** the map management page MUST be selected in the shell navigation

### Requirement: Double-click tray shows the main window
Double-clicking the tray icon MUST show and activate the main window.

#### Scenario: User double-clicks the tray icon
- **WHEN** the user double-clicks the tray icon while the main window is hidden
- **THEN** the main window MUST become visible and activated
