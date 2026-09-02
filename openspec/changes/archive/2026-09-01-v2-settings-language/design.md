## Context

See `proposal.md` for motivation. v2 shell currently lists 地图 / 微程序 / 工具箱 in one ListBox with hardcoded Chinese strings. Persistence for nano already uses `Ra3MapUtils.v2.db` via `AppDataPaths`; legacy `SettingsDAO` writes `Ra3MapUtils.db` and must not be reused for UI language. Product decisions: S1+S2+S3 string coverage; immediate apply; first-run follows OS; settings options are only zh-CN / en (option B); settings nav pinned at bottom.

## Goals / Non-Goals

**Goals:**
- Bottom-pinned Settings page + language control with v2.db persistence.
- ResourceDictionary (or equivalent) hot-swap so shell, pages, tool windows, and app dialogs switch without restart.
- First-run OS culture mapping; thereafter saved explicit language.

**Non-Goals:**
- Legacy settings migration (update, Lua paths, API toggles).
- “Follow system” as a persisted third choice after the user has selected a language.
- Perfect localization of third-party or CLR exception text.

## Decisions

### 1. Shell layout: feature list + bottom-pinned Settings
- **Choice:** Split the left rail: primary `ListBox`/`ItemsControl` for maps/nano/toolbox; a fixed bottom button/item for Settings (DockPanel bottom).
- **Why:** Matches Windows Settings-like pattern and the agreed pin; avoids Settings scrolling away in a long list.
- **Alternatives:** Fourth list item (simpler, weaker pin).

### 2. Localization via paired ResourceDictionaries + DynamicResource
- **Choice:** `Strings.zh-CN.xaml` / `Strings.en.xaml` (or merged dictionaries) with keyed strings; UI binds `DynamicResource`; ViewModels use a small `ILocalizationService` / locator for code-behind and MessageBox text. On language change, replace the active dictionary in `Application.Current.Resources.MergedDictionaries` and raise a change notification for VM-bound titles.
- **Why:** Native WPF, hot-swappable, no new third-party i18n package.
- **Alternatives:** `.resx` + culture (weaker live swap); third-party loc extensions (extra dependency).

### 3. Persistence in v2 SQLite, not SettingsDAO
- **Choice:** Store `UI_Language` (`zh-CN` | `en`) in `Ra3MapUtils.v2.db` (dedicated settings table or kv). Absent key = first-run → resolve from `CultureInfo.CurrentUICulture` (`zh*` → zh-CN, else en) without writing until the user changes the control (or optionally write the resolved value on first apply—prefer **write only on user selection** so “no key” keeps meaning first-run follow system across installs that never opened Settings).
- **Why:** Aligns with v2 DB policy; option B (no Follow-system item) still needs “unset” vs “chosen”.
- **Alternatives:** Always write resolved language on first launch (then OS changes never matter even before user opens Settings—acceptable but weaker “follow system until chosen”).

### 4. Immediate apply pipeline
- **Choice:** Settings VM command → save if user-selected → `ILocalizationService.SetLanguage` → swap dictionaries → update nav item display names / window titles that are VM properties.
- **Why:** Spec requires no restart for covered strings.
- **Alternatives:** Restart prompt (rejected by product).

### 5. Coverage strategy
- **Choice:** Inventory and migrate strings in batches: shell → three pages → toolbox windows (incl. developer hosting) → MessageBox/dialog helpers. Prefer resources for XAML; centralize dialogs through a helper that reads localized strings.
- **Why:** S3 is large; tasks stay trackable without splitting into multiple changes.

### 6. Culture for formatting
- **Choice:** Keep invariant culture for hashes/numbers where already used; only UI language resources change. Optionally set `Thread.CurrentThread.CurrentUICulture` to the selected language for any framework lookups.
- **Why:** Avoid breaking FastHash/hex formatting.

## Risks / Trade-offs

- [Hardcoded string misses] → Mitigate with a sweep checklist in tasks; grep for Chinese literals and MessageBox.
- [Open windows not refreshing] → Mitigate DynamicResource for XAML; for VM strings subscribe to language-changed.
- [Unset vs saved semantics] → Mitigate: only persist when user changes ComboBox; document in settings UI lightly if needed.
- [Large PR] → Mitigate batched tasks; keep behavior scoped to localization + settings shell.

## Migration Plan

1. Add settings store + localization service + dictionaries (zh/en).
2. Pin Settings in shell; wire language control.
3. Migrate shell + pages + tools + dialogs to resources.
4. Verify first-run OS default, persist-after-select, hot-swap, bottom pin.
5. Rollback: remove nav pin and resources; revert to Chinese literals if needed.

## Open Questions

- Whether first successful start should leave the key unset (recommended) or eagerly persist the OS-resolved language—resolve during apply in favor of **unset until user selects**, unless UX testing shows the ComboBox needs an initial explicit value (display can still show the resolved language without writing).
