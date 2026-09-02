## Why

v2 UI strings are hardcoded Chinese and there is no settings surface. English-speaking (and bilingual) users need a first-class language preference, and the Fluent shell needs a settings page as the home for that preference and future options—without waiting for a full legacy settings port.

## What Changes

- Add a Settings page pinned at the bottom of the left navigation (distinct from the scrollable feature items).
- Add UI localization for Simplified Chinese (`zh-CN`) and English (`en`), covering shell, feature pages, toolbox tool windows, and user-visible dialogs/messages (S1+S2+S3).
- Language applies immediately when changed (resource hot-swap); no app restart required for covered strings.
- First launch with no saved preference follows the OS language (`zh*` → Simplified Chinese, otherwise English). The settings control offers only two choices (简体中文 / English)—no explicit “Follow system” option. Once the user picks a language, that choice is persisted and no longer tracks OS changes.
- Persist the language preference in the v2 AppData database (`Ra3MapUtils.v2.db`), not the legacy `Ra3MapUtils.db`.
- **Non-goals:** Porting legacy settings (update, Lua lib paths, etc.); adding more languages; localizing OS/third-party exception text that the app does not own.

## Capabilities

### New Capabilities
- `app-settings`: Settings page shell, bottom-pinned navigation entry, and the language preference control including persistence semantics.
- `ui-localization`: Language resolution (first-run system default vs saved choice), resource loading/hot-swap, and coverage of shell, pages, tool windows, and dialogs.

### Modified Capabilities
- `v2-app-shell`: Navigation MUST include a settings item pinned at the bottom of the left rail in addition to maps, nano programs, and toolbox.

## Impact

- `src/UI` shell (MainWindow layout/nav), new Settings page + ViewModel, localization resources and a small localization/settings service.
- `src/Core` (or UI) persistence for UI settings in `Ra3MapUtils.v2.db`.
- Broad string migration across existing pages and toolbox windows so DynamicResource/bindings can switch at runtime.
- Spec updates for shell navigation occupancy (settings always present alongside the three feature entries).
