## Context

See `proposal.md` for motivation. Legacy nano programs live in `Ra3MapUtils` + `SharedFunctionLib` with SQLite table `nano_program_meta` in `Ra3MapUtils.db`, ScriptExecutor, and ASP.NET on `127.0.0.1:30033`. v2 currently has map UI/Core only, no scripting host or HTTP. Product decisions: usable vertical slice; new DB filename (not `Ra3MapUtils.db`); REST required; dialogs complete (WinForms OK transitional); MapFilePath behavior legacy-compatible; Mutex `Ra3MapUtils`; port `30033`.

## Goals / Non-Goals

**Goals:**

- Ship a runnable 微程序 page + execution host + v2 metadata DB + HTTP nano routes.
- Keep official packages working with Dreamness APIs and interactive dialogs.
- Single-instance + fixed port so WB/tools keep calling localhost:30033.

**Non-Goals:**

- Store/marketplace install flows.
- MCP nano-program tools.
- Importing enable/order from legacy `Ra3MapUtils.db`.
- Wiring map-page selection into MapFilePath for this change.
- Replacing WinForms script dialogs with WPF in this change.

## Decisions

### 1. Layout: Core owns catalog/meta; UI hosts executor + HTTP

- **Choice**: `src/Core` for discovery, v2 SQLite meta, execution orchestration interfaces; `src/UI` references Dreamness/ScriptExecutor, hosts ASP.NET Core, presents pages/dialogs.
- **Why**: Matches existing UI/Core split; HTTP and WPF share one process like legacy.
- **Alternatives**: Separate Hosting project (extra csproj without strong need yet).

### 2. Database file: `Ra3MapUtils.v2.db`

- **Choice**: `%AppData%/Ra3MapUtils/Ra3MapUtils.v2.db` with a `nano_program_meta` table (id, is_enabled, is_wb_visible, order_num).
- **Why**: Explicitly not `Ra3MapUtils.db`; simple and discoverable.
- **Alternatives**: `v2/app.db` subdirectory (also fine; prefer single clear filename unless path helpers already want a v2 folder).

### 3. Dialogs: enable WinForms on UI project; reuse/port ScriptViews

- **Choice**: `<UseWindowsForms>true</UseWindowsForms>`; bring `MapFileSelectorDialog` / message dialogs into v2 (copy or shared folder) so official Main.cs keeps compiling against familiar types where possible (`Ra3MapUtils.ScriptViews` namespace preserved or type-forwarded).
- **Why**: Fastest path to “directly usable”; WPF rewrite is a later change.
- **Alternatives**: Full WPF dialogs now (higher cost).

### 4. Namespace / assembly surface for scripts

- **Choice**: Keep script-facing helper types under namespaces official scripts already `using` (e.g. `Ra3MapUtils.ScriptViews`, path utils as needed), even inside the UI project, OR adjust only if all Official Main.cs are updated in the same change.
- **Why**: Official packages are source text loaded at runtime; breaking usings breaks runs.
- **Alternatives**: Mass-edit every Main.cs (possible but noisy).

### 5. Single instance + port

- **Choice**: Mutex name `Ra3MapUtils` at process start (before UI/HTTP). Kestrel/ASP.NET listens on `http://127.0.0.1:30033`. On Mutex failure, exit (optionally activate existing window later—nice-to-have).
- **Why**: Matches product decision; prevents dual listeners.
- **Alternatives**: Different Mutex (would allow legacy+v2 fight over 30033).

### 6. HTTP surface

- **Choice**: Mirror legacy routes under `/api/nanoprogram`: `GET list`, `GET wb_visible_list`, `POST run/{id}` with `Dictionary<string,string>` body. Prefer JSON results without modal UI on the HTTP path (improve over legacy MsgDialog-in-API if practical).
- **Why**: WB compatibility; cleaner server behavior.
- **Alternatives**: New route prefix (would break clients).

### 7. Ordering UX

- **Choice**: Support reorder with persist (drag-drop if feasible; otherwise up/down commands). Must write `order_num` to v2 DB.
- **Why**: Legacy users expect order control.
- **Alternatives**: Defer reorder (rejected—management completeness).

### 8. Package assets

- **Choice**: Copy `Ra3MapUtils/data/nano_programs/**` into UI output via csproj rules; exclude Main.cs from compile.
- **Why**: Same packaging model as legacy.

## Risks / Trade-offs

- [STA / dialogs from background tasks] → Run script execution with UI marshaling patterns consistent with legacy; document known risks.
- [Full host assembly injection = high privilege] → Same trust model as legacy; only load intended packages.
- [Namespace mismatch breaks Official scripts] → Verify at least one map-touching and one dialog-touching package during verification.
- [Port bind failure if orphan process] → Surface clear error; Mutex should prevent common case.
- [WinForms + WPF mix] → Accepted transitional debt.

## Migration Plan

1. Implement v2 nano + hosting beside map features; users run `src/UI` as Ra3MapUtils.exe.
2. Legacy app remains in repo but cannot run concurrently (shared Mutex).
3. Later: WPF dialogs, optional meta import from old DB, store install, packaging cutover.

Rollback: remove nano page/hosting/DB usage; map features unaffected.

## Open Questions

- Whether second-instance should foreground the existing window (behavior polish only; does not change specs).
