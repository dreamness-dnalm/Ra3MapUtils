# Implementation Plan: Ra3MapUtils Current Architecture Baseline

**Branch**: `[001-current-architecture-baseline]` | **Date**: 2026-05-13 | **Spec**: `specs/001-current-architecture-baseline/spec.md`  
**Input**: Feature specification from `/specs/001-current-architecture-baseline/spec.md`

## Summary

Introduce spec-kit into the existing Ra3MapUtils repository with the official Codex integration, then establish a Chinese-first baseline specification for current architecture and future spec-driven development. This is a documentation and workflow change only; it does not alter runtime code, public APIs, database schemas, map file behavior, or packaging.

## Technical Context

**Language/Version**: C# with .NET 8 WPF for `Ra3MapUtils`; .NET Framework 4.5 for `SharedFunctionLib` and `UtilCoreLib`; .NET 8 for `KnowledgeBaseLib` and `KnowledgeBaseCli`  
**Primary Dependencies**: WPF-UI, HandyControl, CommunityToolkit.Mvvm, ASP.NET Core embedded hosting, MCP attributes, SQLite/linq2db, SQLite FTS/Jieba tokenizer, proprietary Dreamness RA3 map libraries  
**Storage**: `%AppData%/Ra3MapUtils/Ra3MapUtils.db`, `%AppData%/Ra3MapUtils/knowledge_base/knowledge_base.db`, map directories and project-local resource files  
**Testing**: Project-level `dotnet build ... --no-restore`, spec-kit `specify version`, spec-kit `specify check`, and targeted manual/route/tool checks as needed  
**Target Platform**: Windows desktop, PowerShell scripts, local embedded HTTP server on `127.0.0.1:30033`  
**Project Type**: Brownfield desktop app with shared libraries, embedded API/MCP service, plugin resources, and CLI subproject  
**Performance Goals**: Preserve current startup, map operation, script import, API/MCP, and knowledge search behavior; no runtime code path is modified by this baseline  
**Constraints**: Keep Chinese text UTF-8; preserve net45 compatibility in legacy libraries; avoid full-solution build as default verification; do not rewrite architecture  
**Scale/Scope**: Baseline covers repository-level workflow and current major modules, not a detailed line-by-line system model

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Layered boundaries**: PASS. Baseline records `View/ViewModel -> Service -> Business -> DAO -> 数据` and requires future tasks to preserve that chain.
- **Compatibility and persistence**: PASS. Baseline requires net45-compatible changes for legacy libraries and explicit upgrade paths for database/schema/config changes.
- **Map/plugin side effects**: PASS. Baseline marks map file operations, plugin installation, micro-program execution, and script import as high-risk.
- **Spec-first traceability**: PASS. Baseline defines `$speckit-*` workflow and concrete task requirements.
- **Minimum validation**: PASS. Baseline records project-level build commands and warning-noise handling without requiring unrelated full-solution validation.
- **No architecture rewrite**: PASS. Baseline is documentation/workflow only and does not change runtime behavior.

## Project Structure

### Documentation (this feature)

```text
specs/001-current-architecture-baseline/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── README.md
├── spec.md
└── tasks.md
```

### Source Code (repository root)

```text
.specify/
├── memory/
│   └── constitution.md
├── templates/
├── scripts/
├── integrations/
├── extensions/
└── workflows/

.agents/
└── skills/
    ├── speckit-constitution/
    ├── speckit-specify/
    ├── speckit-plan/
    ├── speckit-tasks/
    ├── speckit-implement/
    ├── speckit-clarify/
    ├── speckit-analyze/
    └── speckit-checklist/

Ra3MapUtils/
SharedFunctionLib/
UtilCoreLib/
KnowledgeBaseLib/
KnowledgeBaseCli/
```

**Structure Decision**: Use a single baseline spec directory to describe current architecture and governance. Do not create separate feature directories for each existing module until a concrete feature or modernization effort needs its own spec.

## Implementation Notes

- Install Specify CLI from the official GitHub repository pinned to `v0.8.7`.
- Initialize the existing repository with `specify init --here --force --integration codex --script ps`.
- Replace the generated constitution placeholder with repository-specific principles derived from the root and submodule agent rules.
- Add the baseline spec package under `specs/001-current-architecture-baseline/`.
- Keep all baseline artifacts documentation-only; no C# source, XAML, JSON runtime resources, database files, or packaging scripts are modified by this feature.

## Public API / Interface Changes

- No runtime public API changes.
- No HTTP route changes.
- No MCP tool changes.
- No database schema changes.
- No plugin or nano-program metadata changes.
- New developer-facing interface: Codex spec-kit skills invoked as `$speckit-constitution`, `$speckit-specify`, `$speckit-clarify`, `$speckit-plan`, `$speckit-tasks`, `$speckit-analyze`, `$speckit-checklist`, and `$speckit-implement`.

## Complexity Tracking

No constitution violations are introduced. The only non-runtime structural addition is `.specify/`, `.agents/skills/`, and `specs/001-current-architecture-baseline/`, which are required by spec-kit.
