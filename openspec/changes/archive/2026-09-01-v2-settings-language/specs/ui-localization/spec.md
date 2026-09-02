## Purpose

Localizes the v2 desktop UI between Simplified Chinese and English with immediate language switching across the shell, feature pages, toolbox windows, and user-visible dialogs.

## ADDED Requirements

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
