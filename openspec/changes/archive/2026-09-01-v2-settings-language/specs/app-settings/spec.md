## Purpose

Provides the v2 settings page as a dedicated surface for user preferences, starting with UI language, accessed from a bottom-pinned shell navigation entry.

## ADDED Requirements

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
