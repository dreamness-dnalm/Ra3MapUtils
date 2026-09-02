## Why

v2 now supports Simplified Chinese / English UI switching, but nano-program list titles and descriptions still come from a single pair of Chinese `Name` / `Description` fields in `info.json`. English UI therefore shows Chinese package labels, and v1 user packages must keep working without forced migration.

## What Changes

- Extend `info.json` metadata with optional parallel English fields (`NameEn`, `DescriptionEn`) while keeping existing `Name` / `Description` as the Chinese (or author-default) values for v1 compatibility
- Resolve displayed name and description from the current UI language with cross-fallback when one side is missing
- Hot-swap displayed nano-program names/descriptions when the UI language changes (no restart)
- Search matches across both Chinese and English name/description fields
- Official built-in packages get English name/description filled in; user packages are not required to provide English
- List/API surfaces that expose nano-program name/description return the language-resolved values for the current UI language
- Out of scope: translating strings inside `Main.cs` / script dialogs; additional languages beyond zh-CN/en; changing sort policy

## Capabilities

### New Capabilities

- *(none)*

### Modified Capabilities

- `nano-programs`: bilingual package metadata, language-aware display/search, and current-language name/description on list/API responses

## Impact

- `src/Core/NanoPrograms` models and catalog/display resolution
- `src/UI` nano-programs page ViewModel (display, search, `LanguageChanged`)
- Official packages under `Ra3MapUtils/data/nano_programs/*/info.json` (add English fields)
- HTTP nano-program list endpoints that return `Name` / `Description` (resolved for current language)
- Nano-program authoring skill / docs mentioning `info.json` shape (optional follow-up)
- No change to enable/visibility/order persistence schema; no requirement that user packages add English fields
