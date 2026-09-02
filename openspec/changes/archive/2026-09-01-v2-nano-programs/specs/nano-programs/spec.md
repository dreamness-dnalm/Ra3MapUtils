## Purpose

Provides a working micro-program (nano program) system in v2: discover packages, persist metadata in a v2 database, run Main.cs scripts, and show results in the UI.

## ADDED Requirements

### Requirement: Discover official and user nano programs
The system MUST discover nano-program packages from the official output directory (`data/nano_programs`) and the user directory under AppData. Each valid package MUST contain `info.json` and `Main.cs`. Store/marketplace scanning is out of scope for this change.

#### Scenario: Official packages appear in the catalog
- **WHEN** the official nano-programs directory contains a valid package with info.json and Main.cs
- **THEN** the catalog MUST include that program using the ID and metadata from info.json

#### Scenario: User packages are discovered
- **WHEN** a valid package exists under the user nano-programs directory
- **THEN** the catalog MUST include it alongside official packages

### Requirement: Persist enable, visibility, and order in a v2 database
Enable flag, WB-visible flag, and display order MUST be stored in a SQLite database file under the Ra3MapUtils AppData folder. The database file name MUST NOT be `Ra3MapUtils.db`.

#### Scenario: Toggle enable is remembered
- **WHEN** the user disables a nano program and restarts the application
- **THEN** that program MUST remain disabled according to the v2 database

#### Scenario: Legacy database file is not reused
- **WHEN** the v2 application stores nano-program metadata
- **THEN** it MUST write to a database file other than `Ra3MapUtils.db`

### Requirement: Nano programs page supports management and run
The UI MUST provide a 微程序 page that lists programs with search, enable/visibility controls, reorder, run, open program folder, open user root folder, and refresh. Running a disabled program MUST be rejected.

#### Scenario: User runs an enabled program
- **WHEN** the user runs an enabled nano program from the page
- **THEN** the system MUST execute that package's Main.cs and present a run result to the user

#### Scenario: Disabled program cannot run
- **WHEN** the user attempts to run a disabled nano program
- **THEN** the system MUST NOT execute the script and MUST indicate the program is disabled

### Requirement: Execution matches legacy argument and host semantics
Script execution MUST expose an argument dictionary to the script (legacy-compatible globals). When the UI/API provides no MapFilePath, behavior MUST remain consistent with legacy (scripts may prompt for a map). The host MUST load the assemblies required for official packages that use Dreamness map APIs and script dialogs.

#### Scenario: Run without MapFilePath argument
- **WHEN** a program is started from the UI without a MapFilePath argument
- **THEN** execution MUST proceed with an empty or absent MapFilePath entry as in legacy
- **AND** scripts that require a map MUST still be able to obtain one through their existing prompt flow

### Requirement: Script dialogs remain usable
Packages that show map-selection or message dialogs MUST remain usable from the v2 host for this change (WinForms-based dialogs are acceptable as a transitional approach).

#### Scenario: Map selector dialog can appear
- **WHEN** a running nano program requests interactive map selection
- **THEN** the user MUST be able to complete or cancel that dialog without crashing the host
