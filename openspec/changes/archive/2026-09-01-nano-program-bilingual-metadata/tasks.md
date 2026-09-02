## 1. Core model and resolution

- [x] 1.1 Add optional `NameEn` / `DescriptionEn` to nano-program info deserialization (`NanoProgramInfoModel`)
- [x] 1.2 Add a shared resolver for display name/description (zh-CN ↔ en cross-fallback, then ID)
- [x] 1.3 Ensure v1 packages without English fields still load and resolve via Chinese fields

## 2. UI list, search, and hot-swap

- [x] 2.1 Bind nano-programs list rows to resolved display name/description (not raw Chinese-only fields)
- [x] 2.2 Update search to match `Name`, `NameEn`, `Description`, and `DescriptionEn`
- [x] 2.3 Refresh list labels (and keep filter coherent) on `ILocalizationService.LanguageChanged`
- [x] 2.4 Use resolved display name in run status / run-result dialog titles

## 3. API mapping

- [x] 3.1 Map HTTP nano-program list responses so exposed name/description are current-language resolved values

## 4. Official package content

- [x] 4.1 Add non-empty `NameEn` / `DescriptionEn` to every official `Ra3MapUtils/data/nano_programs/*/info.json`
- [x] 4.2 Optionally update the nano-program authoring skill example `info.json` to document the new fields

## 5. Verification

- [x] 5.1 Build `src\UI\UI.csproj` successfully
- [x] 5.2 Verify: v1-shaped package still appears; English UI shows English official labels; missing English falls back to Chinese; search hits both languages; language switch updates list without restart; list API name matches current UI language
