## ADDED Requirements

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
