## ADDED Requirements

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
