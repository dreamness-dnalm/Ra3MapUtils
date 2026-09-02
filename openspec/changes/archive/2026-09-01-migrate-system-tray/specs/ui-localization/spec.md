## ADDED Requirements

### Requirement: Tray and second-instance tip strings are localizable
System tray menu labels and the second-instance “already running” tip MUST be localizable for Simplified Chinese and English and MUST follow the current UI language, including after a language change while the application remains running where the tray menu is rebuilt or refreshed.

#### Scenario: Tray menu follows English
- **WHEN** the UI language is English and the user opens the tray context menu
- **THEN** the tray menu action labels MUST appear in English

#### Scenario: Second-instance tip follows language
- **WHEN** the UI language is English and a second launch shows the already-running tip
- **THEN** the tip text MUST appear in English
