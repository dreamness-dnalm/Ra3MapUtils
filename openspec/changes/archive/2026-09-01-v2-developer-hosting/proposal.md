## Why

v2 already hosts `127.0.0.1:30033` with nano-program routes, but Cursor/agents and external scripts still depend on the legacy companion for Swagger, MCP (`/mcp`), and the rest of the HTTP API surface. The toolbox shell is now ready, so developer hosting can land as a catalog entry instead of another top-level nav page.

## What Changes

- Enable Swagger/SwaggerUI and MCP HTTP transport on the existing Kestrel host (`/swagger`, `/mcp`), matching legacy developer-facing endpoints.
- Port the remaining legacy REST controllers into v2 with the same routes and `ApiResponse<T>` contract: status, csharp script, lua4 syntax, lua-import scheme, and image-encoding (nano routes already exist).
- Port legacy MCP tool types: C# script/introspection, map folder listing, map file copy/rename/delete, and Lua syntax/import-scheme tools.
- Add a toolbox catalog entry that opens a small developer-hosting window (endpoint copy, docs links, open Swagger)—not a new shell navigation item.
- **Non-goals:** inventing NanoProgram MCP tools that legacy does not expose; restoring a top-level AI navigation page; changing the Mutex name or port `30033`.

## Capabilities

### New Capabilities
- `http-api`: Legacy-compatible REST surface beyond nano (status, csharp, lua4, lua-import, image-encoding) including `ApiResponse` conventions.
- `mcp-tools`: MCP tools exposed at `/mcp` for C# scripting/introspection, map folder/file operations, and Lua syntax/import.
- `developer-hosting-ui`: Toolbox-launched window that surfaces MCP/HTTP endpoints and opens Swagger for humans.

### Modified Capabilities
- `v2-local-hosting`: Host MUST also serve Swagger UI and MCP at `/mcp` on the same loopback listener.
- `toolbox-catalog`: Catalog MUST include a developer-hosting entry that opens the developer hosting window.

## Impact

- `src/UI/App.xaml.cs` HTTP bootstrap (packages: ModelContextProtocol*, Swashbuckle; wire Swagger + MCP).
- New/ported controllers under `src/UI/API/` and MCP types under `src/UI/MCP/` (or equivalent), plus DI for services already used by toolbox (e.g. image encoding) and ScriptExecutor/assembly loading for C# APIs.
- Toolbox catalog/launcher registration and a new tool window/ViewModel.
- External clients (Cursor MCP config, WB/scripts hitting localhost) can target the v2 process once it is the single running instance on port 30033.
