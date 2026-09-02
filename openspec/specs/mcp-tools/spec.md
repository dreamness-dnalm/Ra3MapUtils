## Purpose

Exposes Model Context Protocol tools over the local companion so AI clients can run C# scripts, inspect assemblies, manage maps, and work with Lua import schemes without using the WPF UI.

## Requirements

### Requirement: MCP endpoint serves registered tools
While the application is running, MCP clients MUST be able to connect to the companion MCP HTTP endpoint and discover the registered tool set for this capability.

#### Scenario: MCP endpoint accepts connections
- **WHEN** an MCP client connects to the companion MCP HTTP endpoint on the local loopback listener
- **THEN** the server MUST present the registered MCP tools for scripting, maps, and Lua

### Requirement: CSharp scripting and introspection tools
The MCP tool set MUST include running a C# script and introspecting loaded assemblies (structure, assembly list, method signatures, type details, and enum values) consistent with legacy companion capabilities.

#### Scenario: Run CSharp via MCP
- **WHEN** a client invokes the run C# script tool with script text
- **THEN** the system MUST execute the script and return an execution result

#### Scenario: Introspect assemblies via MCP
- **WHEN** a client invokes an introspection tool for a loaded assembly, type, method, or enum
- **THEN** the system MUST return structured information describing that target or a clear error when it cannot be resolved

### Requirement: Map folder and file tools
The MCP tool set MUST include listing RA3 maps and performing copy, rename, and delete map operations.

#### Scenario: List maps via MCP
- **WHEN** a client invokes the map-list tool
- **THEN** the response MUST include the discoverable RA3 maps available to the companion

#### Scenario: Mutate map via MCP
- **WHEN** a client invokes copy, rename, or delete for a valid map target
- **THEN** the system MUST perform that filesystem map operation and return success or failure

### Requirement: Lua syntax and import-scheme tools
The MCP tool set MUST include checking Lua4 syntax and exporting/importing a map Lua import scheme from JSON.

#### Scenario: Check Lua syntax via MCP
- **WHEN** a client invokes the Lua syntax tool with script text
- **THEN** the response MUST report validity and diagnostics as applicable

#### Scenario: Export or import Lua scheme via MCP
- **WHEN** a client invokes export or import scheme tools with a map path and required scheme inputs
- **THEN** the system MUST perform the corresponding scheme operation and return a result
