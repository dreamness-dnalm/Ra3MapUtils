## Purpose

Lets the v2 companion check for and apply signed AssetCenter releases of itself on the stable channel, with a clear product version surface starting at 2.0.0.

## Requirements

### Requirement: Product version is 2.0.0 for this release line
The running companion MUST expose product version `2.0.0` (or a later SemVer published on the same AssetCenter asset) consistently in UI surfaces that show the application version and in the version metadata used for update eligibility.

#### Scenario: Settings or about shows 2.0.0
- **WHEN** the user views the application version on the settings page (or the designated version surface)
- **THEN** the displayed product version MUST be `2.0.0` for the initial release of this change

### Requirement: User can check for companion updates on stable
The application MUST allow the user to check whether a newer signed companion release is available on the AssetCenter `stable` channel for asset `cn.dreamness.ra3maputils`. This change MUST NOT require a beta channel selector.

#### Scenario: Check finds an update
- **WHEN** the stable channel advertises a newer applicable companion release and the user checks for updates
- **THEN** the UI MUST indicate that an update is available
- **AND** MUST show enough version identity for the user to recognize the target release

#### Scenario: Check finds no update
- **WHEN** the stable channel has no newer applicable companion release and the user checks for updates
- **THEN** the UI MUST indicate that the application is up to date (or that no update is available)

#### Scenario: Check fails clearly
- **WHEN** the update check cannot complete (network, trust, or service failure)
- **THEN** the UI MUST show a clear failure state
- **AND** MUST NOT claim that an update was applied

### Requirement: User can download and apply a companion update
When an update is available, the user MUST be able to download and stage the verified artifact, then apply it through the AssetCenter activation path (exit-and-activate / Bootstrapper) rather than by overwriting the running executable in place.

#### Scenario: Download and stage succeed
- **WHEN** an update is available and the user starts the download
- **THEN** the system MUST download and verify the artifact
- **AND** MUST present progress or an equivalent busy state while work is in progress

#### Scenario: Apply requires restart or exit activation
- **WHEN** a verified update has been staged
- **THEN** applying it MUST schedule activation that does not corrupt the currently running process image
- **AND** the user MUST be guided to exit/restart as required by that activation path

#### Scenario: Download or verify failure
- **WHEN** download or verification fails
- **THEN** the UI MUST report failure clearly
- **AND** the currently running version MUST remain usable

### Requirement: Companion updates do not ship parallel content assets
Companion application releases MUST NOT include the independently versioned Lua library (or other parallel content assets) inside the companion install/update payload. Those assets MAY be updated by later features through separate AssetCenter identities.

#### Scenario: Update payload is companion-only
- **WHEN** a companion stable update is downloaded and applied
- **THEN** that update MUST refresh the companion application
- **AND** MUST NOT be required to install or replace a Lua library tree as part of the same payload
