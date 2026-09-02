## Context

See `proposal.md` for motivation. Legacy toolbox is a WrapPanel of CardActions in `Ra3MapUtils` with per-tool RelayCommands and subwindows under `Views/SubWindows/toolbox`. v2 shell already hosts 地图 and 微程序 via ContentControl + UserControl pages. Product decisions for this change: architecture-first (1A), static registration only, migrate FastHash + image encoding (and optionally one more low-risk tool). Web/MCP deferred.

## Goals / Non-Goals

**Goals:**

- Ship a Fluent 工具箱 page with categories backed by a static ToolEntry catalog.
- One launch pipeline for opening registered tools.
- Prove the path with FastHash and image encoding windows in `src/UI`.

**Non-Goals:**

- Full port of every legacy toolbox window.
- Dynamic/plugin discovery or user-defined tools.
- MCP/HTTP/Swagger toolbox card.
- Merging nano-program packages into the toolbox catalog.
- Deleting the legacy ToolBoxPage in this change.

## Decisions

### 1. Static in-code catalog

- **Choice**: Register tools in code (e.g. a `ToolboxCatalog` list/factory), not filesystem discovery or SQLite.
- **Why**: Matches 1A scope; enough for built-in tools; avoids over-building like nano programs.
- **Alternatives**: JSON/manifest discovery (defer); DB-backed enable/order (not needed yet).

### 2. Core owns models + catalog; UI owns windows and launch

- **Choice**: Put `ToolEntry` / category enums / catalog provider in `src/Core` (or UI-local if Core stays thin); UI implements launchers that open WPF windows.
- **Why**: Same split as maps/nano; keeps page VM free of window construction details.
- **Alternatives**: All UI-only (acceptable if Core has nothing to share yet—prefer Core models for consistency).

### 3. Categories as page sections

- **Choice**: Group on the toolbox page (e.g. 地图与数据 / 其他), not separate nav items.
- **Why**: Keeps left nav sparse; Settings-like single page with sections.
- **Alternatives**: Sub-navigation inside toolbox (unnecessary for 2–3 tools).

### 4. First migrated tools

- **Choice**: FastHash calculator and image encoding tool as required demos; third tool optional only if trivial.
- **Why**: Low external dependency vs debugger/Lua executor/NWB-tied tools.
- **Alternatives**: Migrate all cards as stubs (rejected for 1A).

### 5. Availability hooks (minimal)

- **Choice**: ToolEntry may carry a simple available/reason; first tools always available. Framework present so later tools can grey out.
- **Why**: Spec requires unavailable behavior; implementation can be stubbed for always-on tools.
- **Alternatives**: Skip availability until later (would weaken the contract).

### 6. Boundary vs nano programs

- **Choice**: Document and keep separate: toolbox = built-in launchers; nano = distributable Main.cs packages with v2.db meta.
- **Why**: Prevents catalog merge mistakes in follow-ups.

## Risks / Trade-offs

- [Ported tool windows still depend on legacy packages/APIs] → Prefer tools with few deps; port minimal UI first; note gaps.
- [Static catalog becomes a mega-file] → Group registrations by category files later if needed.
- [Users still need legacy app for other tools] → Accept until incremental migration; do not claim feature parity.
- [Duplicate windows if legacy + v2 both open] → Different process via Mutex already; no shared window state.

## Migration Plan

1. Add toolbox page + catalog + nav; register FastHash + image encoding.
2. Later changes migrate remaining tools into the same catalog.
3. Later change adds developer-hosting entry card (MCP/HTTP) as a toolbox ToolEntry.
4. Eventually remove legacy ToolBoxPage after parity.

Rollback: remove nav item and toolbox types; map/nano unaffected.

## Open Questions

- Whether image encoding needs Magick.NET already referenced by UI (likely yes)—confirm during apply; not a product-scope question.
