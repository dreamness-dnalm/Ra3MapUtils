## Context

See proposal.md for motivation. Nano packages already deserialize `info.json` into `NanoProgramInfoModel` with single `Name` / `Description`. The nano-programs page binds those strings directly and filters search against them only. UI language is already owned by `ILocalizationService` (`zh-CN` | `en`) with a `LanguageChanged` event. HTTP `NanoProgramController` returns `NanoProgramModel` graphs that currently expose raw info fields.

## Goals / Non-Goals

**Goals:**
- Parallel optional `NameEn` / `DescriptionEn` with zero-break for v1 packages
- Central resolution + bilingual search + hot-swap on language change
- Official `info.json` files filled with English copy
- List/API consumers see resolved name/description for the current language

**Non-Goals:**
- Localizing `Main.cs` / script dialogs
- Supporting languages other than zh-CN and en
- Changing enable/visibility/order persistence or discovery roots
- Requiring user packages to ship English metadata
- Changing list sort policy

## Decisions

### Decision: Parallel fields (`NameEn` / `DescriptionEn`)
Keep `Name` / `Description` as the Chinese (author-default) slots used by all existing v1 packages. Add optional `NameEn` / `DescriptionEn` rather than nested locale maps.

**Alternatives considered:** nested `{ "zh-CN": ..., "en": ... }` objects; separate `Locales` block. Rejected for weaker drop-in compatibility with existing JSON and author tooling.

### Decision: Cross-fallback per field
For display: prefer the current-language field; if blank/missing, use the other language’s field; if both blank, fall back to `ID`.

**Alternatives considered:** empty string when preferred language missing; always prefer Chinese. Rejected — user asked for mutual fallback so partial bilingual packages still look complete.

### Decision: Resolve at presentation / response mapping time
Keep raw fields on the deserialized info model. Compute display name/description when building UI list items and when mapping API list responses, using `ILocalizationService.CurrentLanguage`. Subscribe to `LanguageChanged` to refresh UI-bound display properties (and rebuild filter if needed).

**Alternatives considered:** mutate `Info.Name` in place on language change (destructive); put English strings only in app ResourceDictionaries (breaks user packages).

### Decision: API returns resolved values on existing name/description properties
For HTTP list payloads, populate the exposed name/description with resolved strings for the current host UI language. Do not require clients to understand `NameEn`.

**Trade-off:** a client that wanted both languages in one response will not get them unless a future endpoint is added. Acceptable for this change’s explicit requirement.

### Decision: Official content update in-repo
Update every official package under `Ra3MapUtils/data/nano_programs/*/info.json` with English fields as part of implementation tasks (content + code together).

## Risks / Trade-offs

- [Partial English quality] → Prefer clear functional English; iterate copy later without schema change
- [API consumers caching Chinese names] → Document that name/description follow host UI language; rare for local companion API
- [Search false positives across languages] → Acceptable; matches user request for bilingual search
- [Json property naming] → Use `NameEn` / `DescriptionEn` with case-insensitive deserializer already in place

## Migration Plan

1. Ship parser/UI/API that treat English fields as optional (backward compatible).
2. Fill official packages’ English fields in the same release.
3. No user-data migration; no DB schema change.
4. Rollback: omit English fields or revert code; old packages remain valid either way.

## Open Questions

*(none — product decisions locked in explore)*
