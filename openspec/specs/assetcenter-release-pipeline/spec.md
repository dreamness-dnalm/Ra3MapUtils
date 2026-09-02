## Purpose

Provide Ra3MapUtils with a repeatable and auditable AssetCenter release path that keeps signing secrets outside source control while supporting local and arbitrary-CI execution.

## Requirements

### Requirement: Stable asset identity and trust material
The repository SHALL declare one immutable AssetCenter application identity for Ra3MapUtils and SHALL contain only its distributable Ed25519 public key. The corresponding private signing key and management credentials MUST remain outside the repository and generated output.

#### Scenario: Repository trust configuration
- **WHEN** a developer or CI runner reads the checked-in AssetCenter configuration
- **THEN** it identifies `cn.dreamness.ra3maputils`, the production distribution endpoint, the public key identifier, and no private key or credential value

#### Scenario: Missing external private key
- **WHEN** a packaging command runs without a readable external private-key path
- **THEN** it fails before publishing or uploading and explains how to supply the secure file path

### Requirement: Explicit reproducible application packaging
The release automation SHALL require an explicit version, publish the v2 `src/UI` WPF application for `win-x64`, and build and independently verify a signed AssetCenter bundle with `Ra3MapUtils.exe` as its entry point. It MUST NOT infer the version from a Git tag, branch, Release, or Action.

#### Scenario: Successful bundle build
- **WHEN** the caller supplies a valid version, Publisher executable, private key, and required toolchain
- **THEN** the automation produces a verified signed bundle and machine-readable build summary under an ignored artifact directory

#### Scenario: Invalid or implicit version
- **WHEN** the caller omits the version or supplies a value outside the accepted release-version syntax
- **THEN** the automation fails before `dotnet publish` and does not derive a replacement from source-control metadata

### Requirement: Provider-neutral release publication
The publication automation SHALL create an explicitly versioned draft through the AssetCenter management API, upload and reuse verified blocks through Publisher, finalize the immutable signed release, and return the release identifier and manifest location. Credentials MUST be accepted only from a process-scoped environment variable or Publisher's protected credential store.

#### Scenario: CI release finalization
- **WHEN** an authorized runner supplies a verified bundle, explicit version, public/private key paths, and `ASSETCENTER_TOKEN`
- **THEN** it creates, uploads, and finalizes the `windows-x64` artifact without requiring any GitHub runtime API

#### Scenario: Secret-safe failure
- **WHEN** management authentication or upload fails
- **THEN** the script returns a nonzero exit code without echoing the bearer token, private key, or presigned upload URL

### Requirement: Reviewed channel delivery boundary
The repository SHALL provide a script that submits an already signed channel publication and SHALL keep signing and promotion separate from build/upload so beta and stable remain explicit reviewed actions.

#### Scenario: Submit signed beta publication
- **WHEN** an operator supplies a canonical signed publication for the registered beta channel
- **THEN** the script submits it with the scoped credential and reports the publication identifier and projection state

#### Scenario: Reject unsigned input
- **WHEN** the supplied channel document lacks the expected signature envelope or asset/channel identity
- **THEN** the script fails locally without sending it to the control plane

### Requirement: Ra3MapUtils integration documentation
The repository SHALL document the embedded SDK and standalone Updater/Bootstrapper choices for the .NET 10 WPF application, including public trust configuration, installation layout, health confirmation, telemetry consent, beta validation, rollback, and stable promotion.

#### Scenario: Developer follows the onboarding guide
- **WHEN** a Ra3MapUtils developer follows the checked-in guide from a clean checkout with separately supplied tools and secrets
- **THEN** they can build a qualified bundle and understand the remaining reviewed registration, publication, and client-integration steps without accessing a private key from Git
