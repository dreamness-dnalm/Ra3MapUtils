## Purpose

Provides the v2 settings page as a dedicated surface for user preferences, starting with UI language, accessed from a bottom-pinned shell navigation entry.

## Requirements

### Requirement: Settings page is available
The application MUST provide a settings page that users can open from the left navigation. The page MUST be suitable for hosting preference controls including language.

#### Scenario: User opens settings from navigation
- **WHEN** the user selects the settings navigation item
- **THEN** the right content area MUST display the settings page

### Requirement: Language preference control
The settings page MUST offer a language control with exactly two choices: Simplified Chinese and English. The control MUST NOT include a separate “follow system” option.

#### Scenario: Language choices are Simplified Chinese and English
- **WHEN** the user views the language preference on the settings page
- **THEN** the available choices MUST be Simplified Chinese and English only

#### Scenario: Selecting a language persists the preference
- **WHEN** the user selects Simplified Chinese or English
- **THEN** the system MUST save that preference in the v2 AppData database
- **AND** MUST NOT write that preference to the legacy `Ra3MapUtils.db` settings store

### Requirement: Saved language is restored on next launch
When a language preference has been saved, subsequent application starts MUST use that saved language rather than re-deriving from the operating system.

#### Scenario: Relaunch uses saved language
- **WHEN** the user previously saved English and later launches the application
- **THEN** the UI MUST start in English even if the OS UI language is Chinese

### Requirement: Lua redundancy factor preference
The settings page MUST offer a control for the Lua import redundancy factor (a non-negative integer that controls how much trailing padding is appended during Lua import to mitigate truncated script content). The value MUST persist in the v2 AppData database and MUST NOT be stored via SharedFunctionLib, UtilCoreLib, or legacy `Ra3MapUtils.db` settings APIs.

#### Scenario: User saves redundancy factor
- **WHEN** the user sets a redundancy factor on the settings page and saves/applies it
- **THEN** the value MUST be stored in the v2 database
- **AND** subsequent imports in the same or later sessions MUST use that stored factor
- **AND** the preference MUST NOT be written through SharedFunctionLib, UtilCoreLib, or legacy `Ra3MapUtils.db` settings APIs

#### Scenario: Default when unset
- **WHEN** no redundancy factor has been saved yet
- **THEN** imports MUST use a documented default factor (100, matching legacy default behavior)

### Requirement: Settings exposes companion update controls
The settings page MUST provide controls and status for checking companion updates on the AssetCenter stable channel, including the current product version and the outcome of the latest check or download attempt.

#### Scenario: User sees version and check action
- **WHEN** the user opens the settings page
- **THEN** the page MUST show the current product version
- **AND** MUST offer a way to check for updates

#### Scenario: Update status is visible after a check
- **WHEN** a check for updates completes successfully
- **THEN** the settings page MUST reflect whether an update is available or the app is up to date
