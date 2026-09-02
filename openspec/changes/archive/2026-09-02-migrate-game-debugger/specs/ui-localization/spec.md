## ADDED Requirements

### Requirement: Game debugger UI strings are localizable
Toolbox titles/descriptions and user-visible strings in the game debugger, map-path settings, time control, and Lua executor surfaces MUST be localizable for Simplified Chinese and English and MUST follow the current UI language.

#### Scenario: Debugger toolbox labels follow English
- **WHEN** the UI language is English and the user views the toolbox
- **THEN** the game-debugger-related toolbox entry titles and descriptions MUST appear in English

#### Scenario: Map-path window labels follow English
- **WHEN** the UI language is English and the user opens the map-path settings tool
- **THEN** that window’s primary chrome and control labels MUST appear in English
