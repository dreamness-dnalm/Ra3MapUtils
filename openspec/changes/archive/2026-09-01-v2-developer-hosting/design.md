## Context

See `proposal.md` for motivation. v2 already binds `127.0.0.1:30033`, maps nano controllers, and has a static toolbox catalog. Legacy companion (net8) adds Swagger, `MapMcp("/mcp")`, remaining REST controllers under `Ra3MapUtils/API/`, and MCP tools under `Ra3MapUtils/MCP/` using ModelContextProtocol 0.4.0-preview.3. Product decisions: full legacy Web/MCP parity; toolbox opens a small window (not top-level AI nav); port into `src/UI` with route/`ApiResponse` contract alignment.

## Goals / Non-Goals

**Goals:**
- Wire Swagger + MCP on the existing Kestrel host without changing Mutex/`30033`.
- Port remaining REST + MCP tool surface with legacy-compatible paths and tool names where clients depend on them.
- Register a toolbox entry that launches a compact developer-hosting window.

**Non-Goals:**
- NanoProgram-specific MCP tools (legacy does not expose them; HTTP nano stays as-is).
- Reintroducing shell navigation for AI.
- Reworking toolbox architecture beyond one new catalog entry/category.

## Decisions

### 1. Code lands in `src/UI` by porting legacy modules
- **Choice:** Port `API/` (minus duplicate nano if already present) and `MCP/` into `src/UI`, adapting namespaces/DI to v2. Keep `ApiResponse` envelope semantics.
- **Why:** Fastest path to parity; controllers already target ASP.NET Core in-process.
- **Alternatives:** Extract a separate hosting library (cleaner long-term, extra project churn this change).

### 2. Extend existing `StartHttpHostAsync` rather than a second server
- **Choice:** Same `WebApplication` adds Swagger middleware, `AddMcpServer().WithHttpTransport().WithToolsFromAssembly()`, `MapMcp("/mcp")`, and registers services needed by controllers/MCP (image encoding, scripting host helpers).
- **Why:** One port, one process—matches legacy and `v2-local-hosting`.
- **Alternatives:** Sidecar process (rejected; breaks single-instance story).

### 3. Package versions
- **Choice:** Add `ModelContextProtocol` + `ModelContextProtocol.AspNetCore` and `Swashbuckle.AspNetCore`; prefer versions that restore on `net10.0-windows`. Start from legacy pins (0.4.0-preview.3 / 9.0.4) and bump only if restore/runtime requires it.
- **Why:** Behavior parity first; TFM mismatch handled at apply time.
- **Alternatives:** Wait for a newer MCP GA only (delays parity).

### 4. C# scripting host shared with nano where practical
- **Choice:** Reuse ScriptExecutor + loaded-assembly patterns already used by nano execution; port legacy `AssemblyAutoLoader` behavior (BaseDirectory + AppData `Libs`) for MCP/API introspection and run endpoints.
- **Why:** Avoid two divergent script hosts in one process.
- **Alternatives:** Exact file copy of legacy static helpers without sharing (more duplication).

### 5. Toolbox category and launch
- **Choice:** Add a developer/hosting category (or use `Other`) with a `ToolEntry` that opens `DeveloperHostingWindow`. Window content mirrors legacy AI page: MCP URL + transport note, Swagger URL + open action, single-instance note. Reuse shared tool title-bar patterns if already present.
- **Why:** Matches agreed entry morphology; keeps shell nav sparse.
- **Alternatives:** Hyperlink-only card with no window (weaker discoverability for MCP URL copy).

### 6. Capability split in specs
- **Choice:** `http-api` + `mcp-tools` + `developer-hosting-ui` new; extend `v2-local-hosting` and `toolbox-catalog`.
- **Why:** Separates transport hosting, REST contract, MCP tool set, and human UI.

## Risks / Trade-offs

- [MCP preview on net10] → Mitigate by restore/build early; bump package if needed; smoke `/mcp` with Cursor or an MCP client.
- [Large C# introspection port] → Mitigate by porting legacy `CSharpScriptService` largely intact; verify compile against available assemblies.
- [Legacy + v2 both installed] → Mitigate with window copy about single instance; Mutex already blocks two v2s but not necessarily legacy Mutex mismatch.
- [Route drift] → Mitigate by keeping legacy `[Route]`/`[Http*]` paths and tool `Name=` attributes when present.
- [Image/Lua dependencies] → Mitigate by wiring existing v2 image service and legacy Lua helpers already referenced by UI/Util/Shared stacks.

## Migration Plan

1. Add packages and host wiring (Swagger + MCP) with health via `/swagger` and `/api/status/ping`.
2. Port REST controllers and shared `ApiResponse`; confirm nano routes still work.
3. Port MCP tool types; verify tool list on `/mcp`.
4. Add toolbox entry + developer window; manual smoke.
5. Rollback: remove packages/registrations/catalog entry; nano-only host remains.

## Open Questions

- Exact MCP package version that restores cleanly on net10 (resolve during apply).
- Whether companion identity fields in `/api/status/companion` should report v2 assembly version only or also a display name string matching legacy (match legacy fields unless a field is obsolete).
