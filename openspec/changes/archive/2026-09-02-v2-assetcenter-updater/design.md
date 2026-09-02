## Context

See proposal.md for motivation. Publish tooling for `cn.dreamness.ra3maputils` already exists under `eng/assetcenter` (from `integrate-ra3maputils-assetcenter`), and `docs/assetcenter-sdk-integration.md` describes Bootstrapper-first activation. v2 settings currently cover language and Lua redundancy only—no update client. Legacy Velopack remains on the old project and is not the v2 path.

## Goals / Non-Goals

**Goals:**
- Embed AssetCenter client usage for the companion asset on **stable only**, product version **2.0.0**.
- Settings UX for check / download / apply guidance; health confirm if Bootstrapper env is present.
- Structure Core/UI so a second AssetId (Lua, etc.) can add another client later without rewriting the companion path.
- Clean up operator docs/scripts so `eng/assetcenter` is the canonical v2 build→pack→publish→channel flow for `2.0.0`.

**Non-Goals:**
- Implementing Lua library install/update or registering a Lua AssetId.
- Beta channel UI or multi-channel preference.
- Shipping Lua (or other parallel assets) inside the companion bundle.
- Replacing or removing legacy Velopack in the old WPF project in this change.
- Enabling telemetry by default (keep consent-gated / off unless already documented otherwise).

## Decisions

### Decision: One SDK, per-asset clients
Use AssetCenter.Sdk with a thin app facade (e.g. `IAppUpdateService`) bound to `cn.dreamness.ra3maputils`, channel hardcoded `stable`, state under `%LOCALAPPDATA%\Ra3MapUtils\updates\app\` (or equivalent). Future parallel assets get **separate** facades/clients, AssetIds, and state directories—never share sequence/state with the companion client.

**Alternatives considered:** Single multiplexed “asset hub” service in this change. Deferred—Yagni until Lua lands; document the directory/AssetId split now.

### Decision: Stable-only channel for this change
Hardcode or configure `stable` without a settings toggle. Avoid beta UX and dual-publication complexity for `2.0.0`.

### Decision: Bootstrapper activation, not in-place overwrite
Follow the existing integration guide: stage verified artifacts; apply via Updater/Bootstrapper on exit. Dev `dotnet run` builds may only exercise check/download; full apply is validated under Bootstrapper layout when tools are available.

### Decision: Version source of truth
Drive `2.0.0` from explicit MSBuild/`Version` (aligned with AssetCenter `-Version`) and surface the same string in settings. Do not invent a second VERSION file that can drift from publish scripts.

### Decision: Publish script organization (companion-only)
Keep `eng/assetcenter` as the home for companion packaging. Document that pack input is `src/UI` publish output only; add README/operator notes for the `2.0.0` + stable sequence. Do **not** generalize pack scripts to arbitrary content roots in this change—that belongs with the future Lua/multi-asset change—but call out the extension point (parameterized source directory / second `asset.json`) in design so later work does not fork a third pipeline.

### Decision: SDK package reference strategy
Prefer organization private NuGet when available; otherwise a documented relative/ProjectReference or local package path that is **not** an absolute machine-specific path committed to the repo. Fail build clearly if the SDK is missing rather than silently no-oping updates.

## Risks / Trade-offs

- [SDK/Bootstrapper not on public NuGet] → Document acquire path; gate apply tests on tool presence.
- [Dev layout ≠ installed layout] → Separate “check works in dev” from “apply works under Bootstrapper”.
- [Parallel-asset design unused this change] → Keep facade boundaries thin so unused abstraction does not bloat UI.
- [Unarchived `integrate-ra3maputils-assetcenter`] → Avoid duplicating main `assetcenter-release-pipeline` specs; archive that change before or when merging pipeline wording into main specs.

## Migration Plan

1. Add SDK reference + companion update facade + settings UI + strings for `2.0.0` / stable.
2. Health-confirm hook when Bootstrapper env vars are set.
3. Operator pass: build/publish docs for first `2.0.0` stable release (actual channel submit may remain a manual gated step).
4. Later change: Lua AssetId + second client + content install root (out of scope here).

Rollback: feature-flag or settings hide + remove SDK registration if needed; published AssetCenter releases remain immutable (roll forward with higher channel sequence).

## Open Questions

- Exact private feed / local package acquisition for `AssetCenter.Sdk` on the build machine (does not change specs; resolve during apply).
