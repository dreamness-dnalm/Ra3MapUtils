## 1. Persistence and localization core

- [x] 1.1 Add v2 UI settings store in `Ra3MapUtils.v2.db` for `UI_Language` (`zh-CN` | `en`; absent = unset)
- [x] 1.2 Add localization service: resolve first-run from OS (`zh*` → zh-CN else en), load/swap ResourceDictionaries, raise language-changed
- [x] 1.3 Add `Strings.zh-CN` and `Strings.en` resource dictionaries with keys for shell/settings baseline
- [x] 1.4 Apply resolved language at app startup before showing the main window (do not persist until user selects)

## 2. Settings page and shell pin

- [x] 2.1 Add SettingPage + ViewModel with language ComboBox (简体中文 / English only)
- [x] 2.2 Pin Settings at the bottom of the left rail; keep maps/nano/toolbox in the upper list
- [x] 2.3 Wire DI and selection so Settings opens the settings page; language change saves and hot-swaps immediately

## 3. Migrate UI strings (S1–S3)

- [x] 3.1 Migrate MainWindow / navigation / app title & subtitle strings to resources
- [x] 3.2 Migrate MapManage, NanoPrograms, and ToolBox page strings to resources
- [x] 3.3 Migrate toolbox tool windows (FastHash, Image Encoding, Developer Hosting) and title-bar chrome strings to resources
- [x] 3.4 Route application-owned MessageBox/dialog strings through localized resources

## 4. Verification

- [x] 4.1 Build `src\UI\UI.csproj` successfully
- [x] 4.2 Verify: bottom-pinned Settings; first-run follows OS when unset; selecting a language persists and applies immediately; relaunch keeps saved language; English covers shell, pages, a tool window, and a sample dialog
