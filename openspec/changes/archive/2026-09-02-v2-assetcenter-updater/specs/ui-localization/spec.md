## ADDED Requirements

### Requirement: Companion update UI strings are localizable
User-visible strings for companion version display, update check, download/progress, apply/restart guidance, and update failure states MUST be localizable for Simplified Chinese and English and MUST follow the current UI language.

#### Scenario: Update labels follow English
- **WHEN** the UI language is English and the user views companion update controls on settings
- **THEN** those controls’ primary labels and status messages MUST appear in English
