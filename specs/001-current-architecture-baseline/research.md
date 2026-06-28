# Research: Ra3MapUtils Current Architecture Baseline

## Decision 1: Use official spec-kit pinned to v0.8.7

**Decision**: Install `specify-cli` via `uv tool install specify-cli --from git+https://github.com/github/spec-kit.git@v0.8.7`.

**Rationale**: The official repository warns that maintained packages are published from GitHub, not similarly named PyPI packages. Pinning a release avoids unreleased changes from `main`.

**Alternatives considered**:

- Install from latest `main`: rejected because the workflow should be reproducible.
- Use an unofficial package: rejected because it is not maintained by the spec-kit project.

## Decision 2: Use Codex integration only

**Decision**: Initialize with `--integration codex --script ps`.

**Rationale**: The active collaboration environment is Codex, and spec-kit's Codex integration installs skills into `.agents/skills` with `$speckit-*` invocation. PowerShell scripts match the Windows-first repository.

**Alternatives considered**:

- Claude integration: rejected because the requested target is Codex and the repository should not maintain multiple spec-kit integrations by default.
- Generic integration: rejected because it provides less direct support for Codex skills.

## Decision 3: Chinese-first specs with English identifiers

**Decision**: Use Chinese as the primary prose language for constitution, specs, plans, and tasks while keeping code identifiers, paths, commands, HTTP names, and attributes in English.

**Rationale**: The repository contains extensive Chinese documentation and UI text. Keeping identifiers in English preserves exact searchability and avoids accidental translation of APIs.

**Alternatives considered**:

- English-only docs: rejected because it fits upstream templates but not the local collaboration context.
- Fully bilingual docs: rejected for the baseline because it doubles maintenance cost without changing implementation safety.

## Decision 4: Single baseline spec directory

**Decision**: Represent current architecture as one baseline feature `001-current-architecture-baseline` with multiple user stories.

**Rationale**: The goal is to establish a shared current-state contract, not to implement four independent runtime features. A single baseline keeps governance discoverable and avoids inventing separate feature branches for existing modules.

**Alternatives considered**:

- One spec per module: deferred until each module gets a concrete feature or modernization initiative.
- Only constitution, no spec: rejected because the user explicitly asked to backfill current-state specs.

## Decision 5: Documentation-only baseline

**Decision**: Do not modify C#, XAML, JSON runtime resources, database schemas, package scripts, plugin resources, or map utilities as part of this feature.

**Rationale**: The requested change is process and specification adoption. Runtime changes would create unnecessary regression risk and blur the purpose of the baseline.

**Alternatives considered**:

- Add test projects during baseline: rejected because testing architecture should be specified in a dedicated quality or test modernization feature.
- Refactor existing modules to match the baseline: rejected because the baseline should describe current safe boundaries before changing behavior.

## Decision 6: Minimum validation over full-solution validation

**Decision**: Use `specify version`, `specify check`, and future project-level `dotnet build <project>.csproj --no-restore` commands rather than requiring full-solution builds for every change.

**Rationale**: The repository has legacy compatibility and known package warning noise. Project-level validation is faster and better aligned with targeted changes.

**Alternatives considered**:

- Always build `Ra3MapUtils.sln`: rejected because it is slower and noisier than needed for many feature slices.
- No validation for documentation: rejected because spec-kit installation itself should be checked.
