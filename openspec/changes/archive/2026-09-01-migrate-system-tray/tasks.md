## 1. Assets and localization

- [x] 1.1 Add a tray-compatible icon asset to the UI project and ensure it copies to output
- [x] 1.2 Add zh-CN / en strings for tray menu (Show, Maps, Nano, Toolbox, Settings, Exit) and second-instance tip

## 2. Tray host and shell close behavior

- [x] 2.1 Implement WinForms `NotifyIcon` host (create after main window ready; dispose on real shutdown) without HandyControl/WPF-UI
- [x] 2.2 Wire tray context menu: Show, Maps, Nano, Toolbox, Settings, Exit; double-click → Show
- [x] 2.3 Change MainWindow close to cancel + Hide; keep HTTP/MCP running while hidden
- [x] 2.4 Tray Exit disposes icon and shuts down the application so `OnExit` tears down hosting

## 3. Navigation from tray

- [x] 3.1 Expose/show+select navigation from MainWindowViewModel (or thin service) for the four shell destinations
- [x] 3.2 Tray navigation actions Show+Activate and select the matching page; refresh menu labels on language change

## 4. Second-instance activate + tip

- [x] 4.1 Add first-instance waiter for a named activation signal (EventWaitHandle or equivalent)
- [x] 4.2 On Mutex `Ra3MapUtils` already held: signal existing instance, show localized tip, exit without UI/HTTP
- [x] 4.3 On signal: existing instance Shows and Activates the main window

## 5. Verification

- [x] 5.1 Verify: close hides to tray; `:30033` still up; Exit stops process and removes tray icon
- [x] 5.2 Verify: tray menu and double-click show; nav shortcuts open correct pages; EN/zh labels work
- [x] 5.3 Verify: second launch activates existing window, shows tip, and does not bind a second host
