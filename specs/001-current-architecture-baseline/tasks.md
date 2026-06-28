# Tasks: Ra3MapUtils Current Architecture Baseline

**Input**: Design documents from `/specs/001-current-architecture-baseline/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`

**Tests**: This baseline is documentation/workflow only. Runtime tests are not required, but spec-kit initialization checks are required.

**Organization**: Tasks are grouped by user story so future maintainers can see how the baseline maps to repository behavior.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches different files or is read-only.
- **[Story]**: Which baseline user story the task supports.
- Paths are repository-relative.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Install spec-kit and initialize the repository for Codex-based spec coding.

- [ ] T001 Install Specify CLI pinned to `v0.8.7` with `uv tool install specify-cli --from git+https://github.com/github/spec-kit.git@v0.8.7`.
- [ ] T002 Initialize repository with `specify init --here --force --integration codex --script ps`.
- [ ] T003 Confirm Codex skills exist under `.agents/skills/speckit-*`.

---

## Phase 2: Constitution and Governance (Blocking Prerequisites)

**Purpose**: Replace generic template governance with repository-specific rules.

- [ ] T004 [US5] Update `.specify/memory/constitution.md` with Ra3MapUtils layered architecture, compatibility, high-risk areas, UTF-8 rules, validation policy, and governance.
- [ ] T005 [US5] Ensure constitution references `AGENTS.md` precedence rather than duplicating or overriding more specific directory rules.

**Checkpoint**: Future specs can run Constitution Check against repository-specific principles.

---

## Phase 3: Main Application Baseline (Priority: P1)

**Goal**: Document WPF, MVVM, Service, API, MCP, plugin, and nano-program boundaries.

**Independent Test**: Given a main-app feature idea, identify the correct layer and minimum validation command without inspecting unrelated modules.

- [ ] T006 [P] [US1] Document main application behavior in `specs/001-current-architecture-baseline/spec.md`.
- [ ] T007 [P] [US1] Document API/MCP/plugin/nano-program conceptual entities in `specs/001-current-architecture-baseline/data-model.md`.
- [ ] T008 [US1] Record that no runtime API or MCP contracts change in `specs/001-current-architecture-baseline/contracts/README.md`.

**Checkpoint**: Main-app changes can reference this baseline before implementation.

---

## Phase 4: Business and Persistence Baseline (Priority: P1)

**Goal**: Document SharedFunctionLib compatibility, DAO, Settings, SQLite, and migration expectations.

**Independent Test**: Given a config or schema change, identify the DAO/model/mapping/upgrade tasks and validation command.

- [ ] T009 [P] [US2] Document persistence requirements in `specs/001-current-architecture-baseline/spec.md`.
- [ ] T010 [P] [US2] Document SettingsRecord and DatabaseMigration entities in `specs/001-current-architecture-baseline/data-model.md`.
- [ ] T011 [US2] Include SharedFunctionLib validation command in `specs/001-current-architecture-baseline/quickstart.md`.

**Checkpoint**: Persistence changes require explicit compatibility strategy.

---

## Phase 5: Map Utility Baseline (Priority: P1)

**Goal**: Document UtilCoreLib side-effect safety for map directories, map.str, Lua, XML, copy/move/delete/compress, and logging behavior.

**Independent Test**: Given a map file operation, identify path normalization, legal map validation, failure handling, and validation command.

- [ ] T012 [P] [US3] Document map file operation requirements in `specs/001-current-architecture-baseline/spec.md`.
- [ ] T013 [P] [US3] Document MapDirectory and MapTextResource entities in `specs/001-current-architecture-baseline/data-model.md`.
- [ ] T014 [US3] Include UtilCoreLib validation command in `specs/001-current-architecture-baseline/quickstart.md`.

**Checkpoint**: Map-file changes cannot proceed without explicit side-effect handling.

---

## Phase 6: Knowledge Base and CLI Baseline (Priority: P2)

**Goal**: Document KnowledgeBaseLib and KnowledgeBaseCli boundaries for FTS, tokenizer behavior, CLI commands, and main-app integration.

**Independent Test**: Given a knowledge search change, identify whether it belongs in library, CLI, or UI.

- [ ] T015 [P] [US4] Document knowledge base requirements in `specs/001-current-architecture-baseline/spec.md`.
- [ ] T016 [P] [US4] Document KnowledgeDocument entity in `specs/001-current-architecture-baseline/data-model.md`.
- [ ] T017 [US4] Include KnowledgeBaseLib and KnowledgeBaseCli validation commands in `specs/001-current-architecture-baseline/quickstart.md`.

**Checkpoint**: Knowledge base changes can be planned without crossing module boundaries accidentally.

---

## Phase 7: Spec Coding Quickstart and Validation

**Purpose**: Make the workflow repeatable for the next feature.

- [ ] T018 [US5] Document `$speckit-*` workflow in `specs/001-current-architecture-baseline/quickstart.md`.
- [ ] T019 [US5] Record spec-kit adoption decisions in `specs/001-current-architecture-baseline/research.md`.
- [ ] T020 [US5] Run `specify version` and record the result in the final implementation summary.
- [ ] T021 [US5] Run `specify check` and record the result in the final implementation summary.

**Checkpoint**: Repository is ready for the next feature to start with `$speckit-specify`.

---

## Dependencies & Execution Order

- **Phase 1** must complete before writing repository-specific spec-kit documents.
- **Phase 2** must complete before future feature plans rely on Constitution Check.
- **Phases 3, 4, 5, and 6** can be drafted in parallel because they document separate module concerns.
- **Phase 7** depends on the baseline files existing and spec-kit being initialized.

## Parallel Opportunities

- T006, T009, T012, and T015 can be drafted in parallel if multiple maintainers own separate module baselines.
- T007, T010, T013, and T016 can be drafted in parallel because they update distinct conceptual sections.
- Future specs may parallelize user stories only after shared Service/DAO/API contracts are stable.

## Implementation Strategy

1. Establish spec-kit infrastructure.
2. Replace generic constitution placeholders with repository-specific governance.
3. Create one current architecture baseline spec.
4. Add quickstart, research, conceptual data model, contract notes, and tasks.
5. Validate spec-kit availability with `specify version` and `specify check`.

## Notes

- This baseline does not require `dotnet build` because it does not modify runtime code.
- Future runtime features should use the validation commands listed in `quickstart.md`.
- Do not treat this baseline as permission to refactor existing modules without a separate feature spec.
