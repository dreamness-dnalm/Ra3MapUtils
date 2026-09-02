## Why

v2 has a provider-neutral AssetCenter *publish* path but no in-app update client, no product version surface, and still relies on legacy Velopack thinking for “how users get new builds.” Shipping `2.0.0` on `stable` requires embedding the AssetCenter SDK (and activation/health story) while keeping room for independently versioned parallel assets such as the Lua library—without bundling those assets into the companion installer in this change.

## What Changes

- Introduce a v2 **application update** capability powered by `AssetCenter.Sdk` against asset `cn.dreamness.ra3maputils`, **stable channel only** for this change.
- Establish a visible product version of **`2.0.0`** (assembly/informational version + UI), aligned with AssetCenter release SemVer.
- Wire check / download / stage / apply-on-exit (or equivalent Bootstrapper activation) plus clear failure states in settings (or an equivalent settings-adjacent surface).
- Keep user data outside immutable version directories; document and implement health confirmation where Bootstrapper/Updater requires it.
- **Tighten and document** the existing `eng/assetcenter` publish scripts and operator flow so building/publishing `2.0.0` is the canonical path (not Velopack / `dev_tools/package.ps1` for v2).
- **Architecture only for parallel assets**: design the client so additional AssetIds (e.g. Lua library) can share SDK patterns later—separate state directories and no coupling of version lines. **Do not** implement Lua install/update, Lua asset registration, or packing Lua into the companion release in this change.

## Capabilities

### New Capabilities

- `app-updates`: In-app AssetCenter-backed update checks and application of companion releases on the stable channel, including version display and apply/restart behavior.

### Modified Capabilities

- `app-settings`: Settings exposes update controls / status for the companion (stable-only in this change).
- `ui-localization`: Update-related strings are localizable (zh-CN / en).

Release-script organization stays in design/tasks against existing `eng/assetcenter` tooling (the `assetcenter-release-pipeline` capability lives in the still-active `integrate-ra3maputils-assetcenter` change until archived; this change does not invent a duplicate main-spec name).

## Impact

- `src/UI` / `src/Core`: new update service facade, DI, settings UI, version constant/source; possible private NuGet / ProjectReference to AssetCenter SDK packages (not legacy projects).
- `eng/assetcenter` + `docs/assetcenter-sdk-integration.md`: script/operator polish for the `2.0.0` / stable-first workflow.
- Does **not** change legacy Velopack app behavior as a hard requirement; does **not** ship Lua library inside the companion bundle; does **not** add beta channel switching UI in this change.
