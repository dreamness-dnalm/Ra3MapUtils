## Why

v2 already has map management and nano programs, but the legacy toolbox is still a hand-written CardAction wall outside the Fluent shell. Before adding developer hosting entry points (MCP/HTTP), v2 needs a grouped, statically registered toolbox launcher so tools can migrate incrementally without another XAML paste wall.

## What Changes

- Add a left-nav **工具箱** page in the v2 shell (`src/UI`), Settings-like layout with in-page categories.
- Introduce a **static ToolEntry catalog** (code-registered) with id, title, description, category, launch kind, and optional availability/requires metadata.
- Implement a shared launch path for opening toolbox tool windows (and simple actions).
- Migrate **2–3 simple tools** end-to-end to prove the skeleton (default candidates: FastHash calculator, image encoding tool; optional third if low-risk).
- Other legacy toolbox tools remain out of this change (placeholder/future migration OK, not required).
- **Non-goals:** MCP/HTTP/Swagger cards, merging nano programs into toolbox, marketplace/custom tools, rewriting every legacy toolbox window.

## Capabilities

### New Capabilities
- `toolbox-catalog`: Static toolbox tool registry, categories, launch semantics, and the toolbox page behavior for discovering and opening registered tools.

### Modified Capabilities
- `v2-app-shell`: Navigation MUST include a toolbox item (工具箱) in addition to maps and nano programs.

## Impact

- `src/UI`: new ToolBox page/ViewModel, nav wiring, DI, migrated tool windows for the chosen 2–3 tools.
- `src/Core` (optional): shared ToolEntry models / catalog interface if kept UI-agnostic.
- Legacy `Ra3MapUtils` ToolboxPage remains until later cutover; no requirement to delete it in this change.
- Builds on existing Fluent shell; no WPF-UI dependency for chrome.
- Follow-up change can add a “本地服务 / 开发者接入” toolbox entry once hosting/MCP migrate.
