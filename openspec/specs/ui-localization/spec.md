## Purpose

Localizes the v2 desktop UI between Simplified Chinese and English with immediate language switching across the shell, feature pages, toolbox windows, and user-visible dialogs.

## Requirements

### Requirement: First-run language follows the operating system
When no language preference has been saved yet, the application MUST choose Simplified Chinese if the OS UI culture indicates Chinese, and English otherwise.

#### Scenario: Chinese OS defaults to Simplified Chinese
- **WHEN** the application starts with no saved language preference and the OS UI culture is a Chinese culture
- **THEN** the UI MUST use Simplified Chinese

#### Scenario: Non-Chinese OS defaults to English
- **WHEN** the application starts with no saved language preference and the OS UI culture is not Chinese
- **THEN** the UI MUST use English

### Requirement: Language change applies immediately
Changing the language preference MUST update covered UI strings without requiring the user to restart the application.

#### Scenario: Switch language while settings is open
- **WHEN** the user changes the language preference on the settings page
- **THEN** settings page strings and shell navigation labels MUST update to the selected language without restarting

### Requirement: Localization covers shell, pages, tools, and dialogs
User-visible application strings in the main shell, map/nano/toolbox feature pages, toolbox tool windows, and application-owned dialogs or message boxes MUST be localizable for Simplified Chinese and English.

#### Scenario: Feature page labels follow language
- **WHEN** the UI language is English
- **THEN** the maps, nano-programs, and toolbox page chrome and primary labels MUST appear in English

#### Scenario: Toolbox window labels follow language
- **WHEN** the UI language is English and the user opens a registered toolbox tool window
- **THEN** that window’s primary chrome and control labels MUST appear in English

#### Scenario: Application dialogs follow language
- **WHEN** the UI language is English and the application shows an application-owned confirmation or information dialog
- **THEN** the dialog’s application-owned title and message text MUST appear in English

### Requirement: Tray and second-instance tip strings are localizable
System tray menu labels and the second-instance “already running” tip MUST be localizable for Simplified Chinese and English and MUST follow the current UI language, including after a language change while the application remains running where the tray menu is rebuilt or refreshed.

#### Scenario: Tray menu follows English
- **WHEN** the UI language is English and the user opens the tray context menu
- **THEN** the tray menu action labels MUST appear in English

#### Scenario: Second-instance tip follows language
- **WHEN** the UI language is English and a second launch shows the already-running tip
- **THEN** the tip text MUST appear in English

### Requirement: Game debugger UI strings are localizable
Toolbox titles/descriptions and user-visible strings in the game debugger, map-path settings, time control, and Lua executor surfaces MUST be localizable for Simplified Chinese and English and MUST follow the current UI language.

#### Scenario: Debugger toolbox labels follow English
- **WHEN** the UI language is English and the user views the toolbox
- **THEN** the game-debugger-related toolbox entry titles and descriptions MUST appear in English

#### Scenario: Map-path window labels follow English
- **WHEN** the UI language is English and the user opens the map-path settings tool
- **THEN** that window’s primary chrome and control labels MUST appear in English

### Requirement: Companion update UI strings are localizable
User-visible strings for companion version display, update check, download/progress, apply/restart guidance, and update failure states MUST be localizable for Simplified Chinese and English and MUST follow the current UI language.

#### Scenario: Update labels follow English
- **WHEN** the UI language is English and the user views companion update controls on settings
- **THEN** those controls’ primary labels and status messages MUST appear in English
