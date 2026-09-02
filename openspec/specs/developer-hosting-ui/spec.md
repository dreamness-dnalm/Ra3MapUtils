## Purpose

Provides a human-facing developer hosting window launched from the toolbox so users can discover local MCP and HTTP endpoints and open API documentation without a dedicated AI navigation page.

## Requirements

### Requirement: Developer hosting window shows local endpoints
The developer hosting window MUST display the local MCP endpoint URL and transport type, and the HTTP API documentation URL, so users can configure external clients.

#### Scenario: Window shows MCP and HTTP documentation addresses
- **WHEN** the user opens the developer hosting window
- **THEN** the window MUST show the MCP endpoint address and the Swagger/HTTP documentation address for the running companion

### Requirement: User can open HTTP API documentation from the window
The window MUST provide an action that opens the local Swagger/HTTP API documentation in the user's default browser.

#### Scenario: Open Swagger from the window
- **WHEN** the user activates the open-documentation action
- **THEN** the system MUST open the local Swagger UI URL in the default browser

### Requirement: Window warns about single-instance hosting
The window MUST make it clear that only one companion instance should own the local API port so users understand conflicts with another running companion.

#### Scenario: Single-instance note is visible
- **WHEN** the user views the developer hosting window
- **THEN** the UI MUST include a short note that only one Ra3MapUtils companion should be running for the local API and MCP endpoints
