## ADDED Requirements

### Requirement: Main window close hides to tray
The main window close affordance MUST hide the window and keep the shell process alive rather than shutting down the application. Process exit remains available through the system tray Exit action defined by the system-tray capability.

#### Scenario: Close button does not exit the process
- **WHEN** the user closes the main window via the window close button
- **THEN** the main window MUST hide
- **AND** the application process MUST continue running
