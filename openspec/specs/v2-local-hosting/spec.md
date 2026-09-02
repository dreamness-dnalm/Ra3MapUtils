## Purpose

Hosts the v2 desktop process as a single instance and exposes the local HTTP API on port 30033, including nano-program endpoints used by external tools.

## Requirements

### Requirement: Single-instance Mutex matches legacy name
The v2 application MUST use a named Mutex `Ra3MapUtils` so that only one Ra3MapUtils companion process runs. If another instance already holds the Mutex, the new process MUST signal the existing instance to show and activate its main window, MUST show a short localized tip that the companion is already running, and MUST exit without starting a second UI or HTTP listener.

#### Scenario: Second launch is blocked
- **WHEN** one Ra3MapUtils instance is already running and the user launches another
- **THEN** the second process MUST not open a second main window or bind port 30033

#### Scenario: Second launch activates existing window and shows tip
- **WHEN** one Ra3MapUtils instance is already running and the user launches another
- **THEN** the existing main window MUST be shown and activated
- **AND** a localized tip indicating the companion is already running MUST be shown
- **AND** the new process MUST exit without becoming the Mutex holder for a second UI/HTTP host

### Requirement: Embedded HTTP API listens on 127.0.0.1:30033
While running, the application MUST serve an HTTP API bound to `127.0.0.1:30033`.

#### Scenario: API port is reachable
- **WHEN** the v2 application has finished startup successfully
- **THEN** clients MUST be able to reach the local API on port 30033 on the loopback interface

### Requirement: Nano-program HTTP routes are available
The API MUST expose nano-program endpoints compatible with legacy paths for listing all programs, listing WB-visible programs, and running a program by id with a string dictionary body of arguments.

#### Scenario: List nano programs via HTTP
- **WHEN** a client calls the list nano-programs endpoint
- **THEN** the response MUST include discovered programs and their metadata flags needed by clients

#### Scenario: Run nano program via HTTP
- **WHEN** a client posts to the run endpoint for an enabled program id with optional arguments such as MapFilePath
- **THEN** the server MUST execute that program with those arguments and return a result payload indicating success or failure

#### Scenario: Disabled program rejected via HTTP
- **WHEN** a client requests run for a disabled program id
- **THEN** the server MUST NOT execute the script and MUST return a failure indication

### Requirement: Swagger UI is available on the local host
While the application is running, the embedded HTTP host MUST serve interactive OpenAPI/Swagger documentation for the local companion API on a path reachable via the same loopback listener as the API.

#### Scenario: Swagger UI loads
- **WHEN** a client opens the Swagger UI URL on the companion loopback listener
- **THEN** the documentation UI MUST load and describe available HTTP API endpoints

### Requirement: MCP HTTP transport is mapped on the local host
While the application is running, the embedded HTTP host MUST expose an MCP HTTP endpoint on a path compatible with the legacy companion (`/mcp`) on the same loopback listener.

#### Scenario: MCP path is reachable
- **WHEN** the application has finished startup successfully
- **THEN** clients MUST be able to reach the MCP HTTP endpoint at `/mcp` on `127.0.0.1:30033`
