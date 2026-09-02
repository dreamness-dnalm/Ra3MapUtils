## ADDED Requirements

### Requirement: Package metadata supports optional English name and description
Nano-program `info.json` MUST continue to accept `Name` and `Description`. It MUST also accept optional `NameEn` and `DescriptionEn` string fields. Packages that omit the English fields MUST remain valid and discoverable (including v1-authored user packages).

#### Scenario: Legacy package without English fields loads
- **WHEN** a package `info.json` contains `ID`, `Name`, and `Description` but no `NameEn` or `DescriptionEn`
- **THEN** the catalog MUST still include that package

#### Scenario: Package with English fields loads
- **WHEN** a package `info.json` includes `NameEn` and/or `DescriptionEn` in addition to `Name` and `Description`
- **THEN** the catalog MUST include that package and retain both language variants for resolution and search

### Requirement: Displayed name and description follow UI language with cross-fallback
For the current UI language, the system MUST resolve a display name and display description as follows: prefer the field for that language; if missing or blank, use the other language’s corresponding field; if both are missing or blank, fall back to a non-empty package identifier such as the program ID. Only Simplified Chinese and English are supported for this resolution. Changing the UI language MUST update displayed nano-program names and descriptions without requiring an application restart.

#### Scenario: English UI prefers English fields
- **WHEN** the UI language is English and a package has non-empty `NameEn` and `DescriptionEn`
- **THEN** the nano-programs list MUST show `NameEn` and `DescriptionEn` for that package

#### Scenario: English UI falls back to Chinese fields
- **WHEN** the UI language is English and a package has empty or missing `NameEn` but a non-empty `Name`
- **THEN** the list MUST show `Name` as the display name

#### Scenario: Chinese UI falls back to English fields
- **WHEN** the UI language is Simplified Chinese and a package has empty or missing `Name` but a non-empty `NameEn`
- **THEN** the list MUST show `NameEn` as the display name

#### Scenario: Language change updates list labels immediately
- **WHEN** the user switches the UI language while the nano-programs page is open
- **THEN** visible package names and descriptions MUST update to the newly resolved values without restarting

### Requirement: Search matches both language variants
Nano-program search MUST match the user’s query against `Name`, `NameEn`, `Description`, and `DescriptionEn` (case-insensitive), regardless of the current UI language.

#### Scenario: English keyword finds package on Chinese UI
- **WHEN** the UI language is Simplified Chinese and the user searches with a keyword that appears only in `NameEn` or `DescriptionEn`
- **THEN** matching packages MUST still appear in the filtered list

#### Scenario: Chinese keyword finds package on English UI
- **WHEN** the UI language is English and the user searches with a keyword that appears only in `Name` or `Description`
- **THEN** matching packages MUST still appear in the filtered list

### Requirement: List and API responses expose current-language name and description
Surfaces that return nano-program name and description to callers (including the nano-programs page binding model and HTTP list responses that expose those fields) MUST provide the language-resolved display name and description for the current UI language, not a raw dump that forces clients to pick fields.

#### Scenario: HTTP list returns resolved English name
- **WHEN** the UI language is English and a client requests the nano-program list API
- **THEN** each item’s exposed name and description fields MUST be the English-resolved values (with fallback rules applied)

### Requirement: Official packages provide Chinese and English metadata
Built-in official nano programs shipped with the application MUST include non-empty Chinese `Name` / `Description` and non-empty English `NameEn` / `DescriptionEn`. User and store packages are NOT required to provide English fields.

#### Scenario: Official package has English metadata
- **WHEN** an official built-in package is discovered
- **THEN** its `info.json` MUST include non-empty `NameEn` and `DescriptionEn` in addition to `Name` and `Description`

## MODIFIED Requirements

### Requirement: Nano programs page supports management and run
The UI MUST provide a nano-programs page that lists programs with search, enable/visibility controls, reorder, run, open program folder, open user root folder, and refresh. Listed names and descriptions MUST follow the language-resolution rules for the current UI language. Search MUST match both Chinese and English name/description fields. Running a disabled program MUST be rejected.

#### Scenario: User runs an enabled program
- **WHEN** the user runs an enabled nano program from the page
- **THEN** the system MUST execute that package's Main.cs and present a run result to the user

#### Scenario: Disabled program cannot run
- **WHEN** the user attempts to run a disabled nano program
- **THEN** the system MUST NOT execute the script and MUST indicate the program is disabled

#### Scenario: List shows language-resolved labels
- **WHEN** the user views the nano-programs list
- **THEN** each row’s title and description MUST be the resolved display strings for the current UI language
