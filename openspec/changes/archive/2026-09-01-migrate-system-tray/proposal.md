## Why

v2 already runs as a single-instance host for the local HTTP API and MCP, but it has no system tray. Closing the main window exits the process, which drops the always-on local services. Legacy Ra3MapUtils kept the process alive via a tray icon and menu; v2 needs the same close-to-tray behavior plus navigation shortcuts and second-instance activation.

## What Changes

- Add a system tray icon for the running v2 companion (no HandyControl / WPF-UI tray control)
- Closing the main window hides to the tray instead of exiting; HTTP/MCP keep running
- Tray context menu: show main window, jump to Maps / Nano programs / Toolbox / Settings, and Exit
- Double-click tray icon shows the main window
- Second launch while Mutex `Ra3MapUtils` is held: activate the existing main window, show a short “already running” tip, then exit the new process
- Localize tray and second-instance tip strings (zh-CN / en)

## Capabilities

### New Capabilities

- `system-tray`: tray icon, close-to-tray, tray menu navigation shortcuts, double-click show, and coordinated second-instance activate+tip

### Modified Capabilities

- `v2-local-hosting`: second-instance path must activate the existing UI and show a tip before exiting (Mutex name stays `Ra3MapUtils`)
- `ui-localization`: tray menu and second-instance tip strings covered by the localization surface
- `v2-app-shell`: main-window close semantics become hide-to-tray (true exit only via tray Exit)

## Impact

- `src/UI`: App single-instance handoff, MainWindow close/hide/show, tray host (prefer WinForms `NotifyIcon` already enabled), MainWindowViewModel navigation commands from tray
- Icon asset for the tray (legacy used pack icon; v2 must ship a suitable icon)
- Specs: new `system-tray`; deltas for hosting, shell close behavior, localization
- No change to Mutex name `Ra3MapUtils`; no HandyControl / WPF-UI dependency for tray
