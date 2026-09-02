## Context

See proposal.md for motivation. Legacy toolbox already ships Ra3Hacker under `Ra3MapUtils/data/Ra3Hacker` and uses `Ra3Hacker.Sdk` for time control / Lua executor. v2 toolbox currently registers only FastHash, image encoding, and developer hosting. Product constraints: no legacy project references; NewWorldBuilder linkage and map log viewer are abandoned; Injector self-locks so companion does not add a process mutex; UX is multi-card / multi-window (not a single workbench).

## Goals / Non-Goals

**Goals:**
- Four toolbox entries + windows with parity for launch, map settings, time, Lua
- Bundle Ra3Hacker data + SDK into v2 output
- Shared settings/client helpers without SharedFunctionLib
- Localized strings; activate existing tool windows on re-open

**Non-Goals:**
- NWB path/plugins/log viewer
- Companion-enforced Injector single-instance
- Merging the four tools into one tabbed workbench
- Changing companion `:30033` hosting

## Decisions

### Decision: Multi-card, multi-window UX
Keep four toolbox cards and separate windows (legacy shape) for faster migration and independent use of time/Lua while settings stay open.

**Alternatives considered:** Single “调试工作台” with tabs. Deferred; can revisit later without blocking this change.

### Decision: Bundle Ra3Hacker next to the UI output
Copy `data/Ra3Hacker/**` into `src/UI` (or link from a shared `src` data root) with Content CopyToOutput, mirroring legacy packaging. Reference `Ra3Hacker.Sdk` from `src/lib` (copy DLL from legacy `lib` if needed).

### Decision: Core or UI service for settings + process start
Introduce a small `IDebuggerMapSettingsService` / launcher abstraction in Core or UI Services that reads/writes `setting.json` `maps` fields and starts `Ra3Hacker.Injector.exe`. Time/Lua windows create `Ra3HackerClient` from the same settings file path (legacy pattern).

### Decision: No companion Injector mutex
Rely on Injector’s own lock. Companion only starts the process and surfaces missing-binary errors.

### Decision: Window single-instance via existing Window enumeration
On toolbox launch, find an already-open window of that tool type and Activate; otherwise create via DI (same pattern as legacy TimeControl / DebuggerMapSettings).

### Decision: Toolbox icons via existing skill workflow
Each new toolbox card gets a 512×512 asset under UI data/imgs, csproj Resource, and page wiring (follow `ra3-toolbox-tool` skill on apply).

## Risks / Trade-offs

- [Bundle size grows with Ra3Hacker natives] → Accept; required for offline debug
- [setting.json user edits vs shipped default] → Write in output/AppData carefully; prefer editing the shipped copy beside Injector as legacy does, document restart requirement
- [SDK API mismatch if Ra3Hacker build drifts] → Pin SDK + data from the same legacy snapshot
- [Lua executor UI complexity] → Port behavior first; visual polish incremental

## Migration Plan

1. Copy Ra3Hacker data + SDK into v2 project references/output.
2. Implement settings service + four windows/VMs + catalog entries + icons/strings.
3. Wire launcher single-instance activation for each window.
4. Smoke: launch injector; save map settings; time/Lua against a running session.

## Open Questions

*(none — product decisions locked)*
