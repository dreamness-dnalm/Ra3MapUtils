# Quickstart: Spec Coding for Ra3MapUtils

## 1. Confirm spec-kit is available

```powershell
specify version
specify check
```

Expected result: `specify` reports version `0.8.7`, and the project check recognizes the initialized spec-kit project and Codex integration.

## 2. Start a new feature with Codex skills

Use the spec-kit skills from Codex in this order for normal feature work:

```text
$speckit-specify
$speckit-clarify
$speckit-plan
$speckit-tasks
$speckit-analyze
$speckit-implement
```

For small low-risk documentation changes, `$speckit-clarify` and `$speckit-analyze` may be skipped if the spec is already clear. For startup, plugin installation, DAO, map file mutation, API, MCP, or dependency changes, do not skip the risk and consistency checks.

## 3. Write specs in this repository's style

- Use Chinese as the primary prose language.
- Keep paths, commands, class names, attributes, JSON keys, HTTP routes, and database identifiers in English.
- Identify the owning layer before implementation: `View/ViewModel -> Service -> Business -> DAO -> 数据`.
- For API changes, preserve `ApiResponse<T>` unless a spec explicitly defines a breaking change.
- For MCP changes, preserve `[McpServerToolType]` and `[McpServerTool]`.
- For DAO/schema/config changes, document upgrade and historical compatibility.
- For map operations, document path normalization, legal map validation, failure messaging, and rollback expectations.

## 4. Pick the minimum validation command

Use the smallest command that matches the changed layer:

```powershell
dotnet build Ra3MapUtils\Ra3MapUtils.csproj --no-restore
dotnet build SharedFunctionLib\SharedFunctionLib.csproj --no-restore
dotnet build UtilCoreLib\UtilCoreLib.csproj --no-restore
dotnet build KnowledgeBaseLib\KnowledgeBaseLib.csproj
dotnet build KnowledgeBaseCli\KnowledgeBaseCli.csproj
```

If the build output includes `NU190x` or `NU1701`, record whether the warning is known existing noise or newly introduced by the current change.

## 5. Use this baseline as the starting point

Before planning a new feature, read:

```text
.specify/memory/constitution.md
specs/001-current-architecture-baseline/spec.md
```

Then inspect the relevant `AGENTS.md` for the directory being changed. More specific `AGENTS.md` rules override broader repository rules.
