## Purpose

Hosts the v2 desktop process as a single instance and exposes the local HTTP API on port 30033, including nano-program endpoints used by external tools.

## ADDED Requirements

### Requirement: Single-instance Mutex matches legacy name
The v2 application MUST use a named Mutex `Ra3MapUtils` so that only one Ra3MapUtils companion process runs. If another instance already holds the Mutex, the new process MUST exit without starting the UI or HTTP listener.

#### Scenario: Second launch is blocked
- **WHEN** one Ra3MapUtils instance is already running and the user launches another
- **THEN** the second process MUST not open a second main window or bind port 30033

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
