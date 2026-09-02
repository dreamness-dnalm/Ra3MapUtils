## MODIFIED Requirements

### Requirement: Single-instance Mutex matches legacy name
The v2 application MUST use a named Mutex `Ra3MapUtils` so that only one Ra3MapUtils companion process runs. If another instance already holds the Mutex, the new process MUST signal the existing instance to show and activate its main window, MUST show a short localized tip that the companion is already running, and MUST exit without starting a second UI or HTTP listener.

#### Scenario: Second launch is blocked
- **WHEN** one Ra3MapUtils instance is already running and the user launches another
- **THEN** the second process MUST not open a second main window or bind port 30033

#### Scenario: Second launch activates existing window and shows tip
- **WHEN** one Ra3MapUtils instance is already running and the user launches another
- **THEN** the existing main window MUST be shown and activated
- **AND** a localized tip indicating the companion is already running MUST be shown
- **AND** the new process MUST exit without becoming the Mutex holder for a second UI/HTTP host
