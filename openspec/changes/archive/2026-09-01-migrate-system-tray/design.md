## Context

See proposal.md for motivation. v2 shell has maps/nano/toolbox/settings navigation and Mutex `Ra3MapUtils`, but closing the main window ends the process. Legacy used HandyControl `NotifyIcon`; `v2-app-shell` forbids HandyControl/WPF-UI for shell chrome. `src/UI` already enables `UseWindowsForms`.

## Goals / Non-Goals

**Goals:**
- Close-to-tray + Exit-from-tray lifecycle
- Tray menu aligned with the four shell destinations + Show
- Double-click show
- Second-instance activate + tip without starting a second host
- Localized strings

**Non-Goals:**
- Reintroducing HandyControl/WPF-UI
- Autostart with Windows / tray-only startup mode
- Expanding tray menu beyond Show / four nav targets / Exit
- Changing Mutex name away from `Ra3MapUtils`

## Decisions

### Decision: WinForms NotifyIcon for tray
Use `System.Windows.Forms.NotifyIcon` owned by App/MainWindow lifetime (create after main window exists; dispose on real exit). Context menu via `ContextMenuStrip` or equivalent; marshal UI actions to the WPF dispatcher.

**Alternatives considered:** Hardcodet.NotifyIcon.Wpf (extra dependency); P/Invoke shell_notifyicon (more code). Rejected for simplicity given WinForms already referenced.

### Decision: Close cancels shutdown; Exit calls Application.Shutdown
MainWindow `Closing` cancels and `Hide()`. Tray Exit disposes the icon then `Application.Current.Shutdown()` (or equivalent) so HTTP host teardown in `OnExit` still runs.

### Decision: Second-instance IPC via named EventWaitHandle (or equivalent lightweight signal)
First instance waits on a named auto-reset event (or similar) and shows/activates the main window on signal. Second instance, after Mutex failure, sets the event, shows the tip (MessageBox on the short-lived process is acceptable), then exits. Exact primitive may be EventWaitHandle or HWND restore if simpler; behavior contract is activate + tip + no second host.

**Alternatives considered:** Named pipe with payload (overkill for show-only); only MessageBox without activate (rejected by product decision).

### Decision: Tray navigation reuses MainWindowViewModel selection APIs
Tray actions call into the existing shell ViewModel (or a thin tray service that does Show + select navigation key) so Maps/Nano/Toolbox/Settings stay consistent with left-rail navigation.

### Decision: Ship a tray icon asset with the UI project
Provide an `.ico` or convert from existing product artwork; do not depend on legacy HandyControl pack URI alone.

## Risks / Trade-offs

- [Ghost tray icon if process kills without Dispose] → Always dispose on Shutdown/Exit; hook process exit path
- [WinForms menu on wrong thread] → Invoke tray commands on WPF Dispatcher
- [Second-instance tip shown by dying process feels odd] → Accept short-lived MessageBox; existing instance still activates
- [Event name collision across users] → Scope named wait handle with a stable product-prefixed name tied to `Ra3MapUtils`

## Migration Plan

1. Add tray host + icon + localized menu wiring.
2. Change MainWindow close to hide; wire Exit to real shutdown.
3. Replace second-instance MessageBox-only path with signal + tip + exit.
4. Verify close-to-tray keeps `:30033` up; Exit tears it down; second launch activates.

## Open Questions

*(none — product decisions locked)*
