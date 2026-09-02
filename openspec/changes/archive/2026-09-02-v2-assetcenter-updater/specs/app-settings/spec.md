## ADDED Requirements

### Requirement: Settings exposes companion update controls
The settings page MUST provide controls and status for checking companion updates on the AssetCenter stable channel, including the current product version and the outcome of the latest check or download attempt.

#### Scenario: User sees version and check action
- **WHEN** the user opens the settings page
- **THEN** the page MUST show the current product version
- **AND** MUST offer a way to check for updates

#### Scenario: Update status is visible after a check
- **WHEN** a check for updates completes successfully
- **THEN** the settings page MUST reflect whether an update is available or the app is up to date
