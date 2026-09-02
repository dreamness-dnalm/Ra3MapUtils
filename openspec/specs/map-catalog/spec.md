## Purpose

Provides listing of real RA3 map directories so the map management page can show the user's actual maps folder contents.

## Requirements

### Requirement: Enumerate maps from the RA3 maps folder
The system MUST enumerate subdirectories of the configured RA3 maps root folder and treat a directory as a map only when it contains a core map file named `{folderName}.map` inside that directory.

#### Scenario: Valid map directories are returned
- **WHEN** the maps root contains a subdirectory `MyMap` that includes `MyMap.map`
- **THEN** the catalog MUST include an entry for `MyMap`

#### Scenario: Non-map directories are excluded
- **WHEN** the maps root contains a subdirectory without a matching `{folderName}.map` file
- **THEN** the catalog MUST NOT include that subdirectory as a map

### Requirement: Map management page shows the live catalog
The map management page MUST display the maps returned by the catalog for the current maps root. The user MUST be able to refresh the list.

#### Scenario: Page load populates the list
- **WHEN** the map management page is shown and the maps root exists
- **THEN** the UI MUST show the current map entries from disk

#### Scenario: User refreshes the list
- **WHEN** the user triggers refresh on the map management page
- **THEN** the UI MUST reload map entries from disk and update the displayed list

### Requirement: Missing or unreadable maps root is reported safely
If the maps root does not exist or cannot be read, the system MUST NOT crash. The UI MUST present an empty list or a clear error state describing the problem.

#### Scenario: Maps root directory missing
- **WHEN** the configured maps root path does not exist
- **THEN** the catalog MUST return no maps (or a failure result the UI can display)
- **AND** the application MUST remain responsive
