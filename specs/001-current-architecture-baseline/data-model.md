# Data Model: Ra3MapUtils Current Architecture Baseline

This baseline does not introduce runtime database tables or serialized application data. The entities below are conceptual documentation entities used by future specs and plans.

## SpecArtifact

- **Purpose**: Captures the intent, plan, tasks, contracts, data model, quickstart, and research for one feature or baseline.
- **Key attributes**: feature number, feature slug, status, created date, user stories, requirements, validation commands.
- **Relationships**: References one or more ApplicationLayer entries and may define HighRiskChange entries.

## ApplicationLayer

- **Purpose**: Names the architectural layer that owns a behavior.
- **Known values**: View/ViewModel, Service, API, MCP, Business, DAO, Data, PluginResource, NanoProgramResource, MapUtility, KnowledgeBaseLibrary, CLI.
- **Relationships**: A Task should identify the ApplicationLayer it modifies; a ValidationCommand should match the modified layer.

## ServiceContract

- **Purpose**: Represents an interface/implementation pair in `Ra3MapUtils/Services`.
- **Key attributes**: interface path, implementation path, DI registration location, caller path.
- **Relationships**: API controllers, MCP tools, and ViewModels may depend on ServiceContract entries.

## ApiEndpoint

- **Purpose**: Represents an embedded HTTP API route.
- **Key attributes**: controller path, route, method, request shape, response shape, `ApiResponse<T>` usage.
- **Relationships**: May call ServiceContract or Business functions; should be documented when changed.

## McpTool

- **Purpose**: Represents an MCP tool exposed to AI clients.
- **Key attributes**: tool class, method, `[McpServerToolType]`, `[McpServerTool]`, description, input/output contract.
- **Relationships**: May call ServiceContract or script execution services.

## PluginPackage

- **Purpose**: Represents a NewWorldBuilder plugin resource.
- **Key attributes**: `plugin_meta.json`, `Main.cs`, `readme.txt`, required file mapping, output copy behavior.
- **Relationships**: Installed by plugin services and may overwrite external editor script files.

## NanoProgram

- **Purpose**: Represents a C# script-based micro-utility.
- **Key attributes**: `info.json`, `Main.cs`, ID, name, description, author, execution order, enablement state.
- **Relationships**: Discovered and executed by nano-program services; may be exposed through API or MCP.

## SettingsRecord

- **Purpose**: Represents persisted configuration in the main application database.
- **Key attributes**: module-prefixed key, value, default behavior, historical compatibility.
- **Relationships**: Managed through SharedFunctionLib DAO/Business code and read by main app services.

## DatabaseMigration

- **Purpose**: Represents a safe schema evolution step.
- **Key attributes**: affected table/model, `InitDB()` behavior, `Upgrade()` branch or equivalent, backfill/default strategy.
- **Relationships**: Required whenever DAO, model, or `SqliteConnection` mappings change.

## MapDirectory

- **Purpose**: Represents a Red Alert 3 map directory under user-controlled storage.
- **Key attributes**: normalized path, `.map` core file presence, related map assets, operation type.
- **Relationships**: Used by map management, Lua import, copy/move/delete/compress operations, and plugin-linked flows.

## MapTextResource

- **Purpose**: Represents map text/script/XML data such as `map.str`, Lua scripts, and XML metadata.
- **Key attributes**: file path, encoding, parser/operator, write behavior, error reporting.
- **Relationships**: Mutated by UtilCoreLib and surfaced through main application services/UI.

## KnowledgeDocument

- **Purpose**: Represents searchable knowledge base content.
- **Key attributes**: document ID, title, body, tokenizer behavior, FTS index state.
- **Relationships**: Stored in the knowledge base SQLite database and accessed by library, CLI, and UI flows.

## HighRiskChange

- **Purpose**: Marks a change that needs explicit risk, compatibility, and validation notes.
- **Known triggers**: startup sequence, DI registration, embedded web server, MCP mapping, plugin installation, map file mutation, DAO/schema/config changes, script import, package/dependency changes.
- **Relationships**: Must be referenced by SpecArtifact and covered by ValidationCommand.

## ValidationCommand

- **Purpose**: Captures the minimum command or manual check for a change.
- **Key attributes**: command text, target project/path, expected signal, known warning noise, whether it is required or optional.
- **Relationships**: Selected based on ApplicationLayer and HighRiskChange.
