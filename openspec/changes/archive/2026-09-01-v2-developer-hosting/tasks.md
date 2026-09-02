## 1. Host packages and transport

- [x] 1.1 Add ModelContextProtocol, ModelContextProtocol.AspNetCore, and Swashbuckle packages to `src/UI` (adjust versions if net10 restore requires)
- [x] 1.2 Extend HTTP host bootstrap: Swagger/SwaggerUI, AddMcpServer + HttpTransport + tools-from-assembly, MapMcp(`/mcp`)
- [x] 1.3 Verify `/swagger` loads and `/api/nanoprogram` routes still work after host changes

## 2. HTTP API surface

- [x] 2.1 Port `ApiResponse` / status codes and status controller (`ping`, `companion`) with legacy routes
- [x] 2.2 Port C# script HTTP endpoints (`run/code`, `run/file`) and wire ScriptExecutor + assembly loading helpers
- [x] 2.3 Port Lua4 syntax HTTP endpoints (`syntax/code`, `syntax/file`)
- [x] 2.4 Port Lua import scheme HTTP endpoints (`scheme/export`, `scheme/import`)
- [x] 2.5 Port image-encoding HTTP endpoints (`preview`, `encode`) onto existing image encoding service
- [x] 2.6 Register any new controller dependencies in the HTTP host DI container

## 3. MCP tools

- [x] 3.1 Port CSharp MCP tools (run + introspection) with legacy tool names where specified
- [x] 3.2 Port map folder list and map file copy/rename/delete MCP tools
- [x] 3.3 Port Lua syntax and import-scheme MCP tools
- [x] 3.4 Confirm MCP tools are discoverable via `/mcp`

## 4. Toolbox developer hosting window

- [x] 4.1 Add DeveloperHosting window + ViewModel (MCP URL, transport note, Swagger URL, open-browser action, single-instance note)
- [x] 4.2 Register catalog entry + launcher path and DI for the window/ViewModel
- [x] 4.3 Manually open toolbox → developer hosting window and open Swagger from it

## 5. Verification

- [x] 5.1 Build `src\UI\UI.csproj` successfully
- [x] 5.2 Smoke: status ping, one non-nano API, Swagger UI, MCP endpoint reachable, toolbox entry launches
