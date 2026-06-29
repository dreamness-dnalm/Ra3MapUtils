# Contracts: Ra3MapUtils Current Architecture Baseline

This baseline introduces no runtime contracts.

- No HTTP routes are added, removed, or changed.
- No MCP tools are added, removed, or changed.
- No database schema or migration contract is added.
- No plugin or nano-program metadata schema is changed.
- No CLI argument contract is changed.

Future feature specs that change runtime contracts should add concrete contract files in this directory or in their own feature directory, such as:

- `openapi.yaml` for HTTP API route changes.
- `mcp-tools.md` for MCP tool input/output changes.
- `cli.md` for KnowledgeBaseCli command changes.
- `migration.md` for DAO/schema/config compatibility changes.
- `plugin-metadata.md` for plugin or nano-program metadata changes.
