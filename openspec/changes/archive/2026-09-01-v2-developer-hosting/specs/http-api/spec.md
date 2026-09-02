## Purpose

Exposes the legacy-compatible REST API surface (beyond nano programs) so external tools and agents can call status, scripting, Lua, and image-encoding endpoints on the local companion.

## ADDED Requirements

### Requirement: Unified API response envelope
Non-nano HTTP JSON endpoints introduced by this capability MUST return a business response envelope with a numeric code, message, and data payload compatible with the legacy companion contract callers already use.

#### Scenario: Successful ping uses success envelope
- **WHEN** a client calls the status ping endpoint successfully
- **THEN** the response MUST use the shared success envelope with a recognizable success business code and payload

#### Scenario: Failed operation uses failure indication
- **WHEN** a client request fails for a handled business reason
- **THEN** the response MUST indicate failure via the shared envelope rather than only an opaque transport error

### Requirement: Status endpoints are available
The API MUST expose status endpoints for a lightweight ping and companion identity/version information on paths compatible with the legacy companion.

#### Scenario: Ping responds
- **WHEN** a client calls the status ping endpoint while the application is running
- **THEN** the response MUST indicate the service is alive

#### Scenario: Companion identity is returned
- **WHEN** a client calls the companion status endpoint
- **THEN** the response MUST include identifying information suitable for external tools to recognize the companion and its version

### Requirement: CSharp script HTTP endpoints are available
The API MUST expose endpoints to run C# script from inline code and from a file path, returning an execution result payload.

#### Scenario: Run inline CSharp
- **WHEN** a client posts C# source to the run-code endpoint
- **THEN** the system MUST execute the script in the companion scripting host and return a success or failure result payload

#### Scenario: Run CSharp from file
- **WHEN** a client posts a readable script file path to the run-file endpoint
- **THEN** the system MUST execute that file's contents and return a success or failure result payload

### Requirement: Lua4 syntax HTTP endpoints are available
The API MUST expose endpoints to check Lua4 syntax from inline code and from a file path.

#### Scenario: Check inline Lua syntax
- **WHEN** a client posts Lua source to the syntax-code endpoint
- **THEN** the response MUST report whether the syntax is valid and include diagnostic detail when invalid

#### Scenario: Check Lua file syntax
- **WHEN** a client posts a readable Lua file path to the syntax-file endpoint
- **THEN** the response MUST report syntax validity for that file's contents

### Requirement: Lua import scheme HTTP endpoints are available
The API MUST expose endpoints to export a map's Lua import scheme to JSON and to import Lua into a map from a JSON scheme, on paths compatible with the legacy companion.

#### Scenario: Export import scheme
- **WHEN** a client requests export for a valid map path
- **THEN** the system MUST produce a JSON scheme payload or file result consistent with legacy behavior

#### Scenario: Import from scheme
- **WHEN** a client requests import with a valid map path and scheme
- **THEN** the system MUST apply the scheme to the map and return success or failure

### Requirement: Image encoding HTTP endpoints are available
The API MUST expose endpoints to preview and encode images according to the legacy image-encoding API contract.

#### Scenario: Preview encoding
- **WHEN** a client posts a preview request with valid image input and options
- **THEN** the response MUST include preview/result data needed by clients without requiring the toolbox UI

#### Scenario: Encode image
- **WHEN** a client posts an encode request with valid image input and options
- **THEN** the system MUST return encoded output data or a failure indication
