## Context

See `proposal.md` for motivation. Ra3MapUtils v2 is the uncommitted `src/UI` + `src/Core` .NET 10 migration on branch `v2`; the legacy `Ra3MapUtils` project already contains Velopack packaging but is not the integration target. AssetCenter production exposes a Worker management API and static R2/CDN distribution at `public-files.dreamness.cn`. Publisher already owns safe source scanning, 7z creation, signing, verification, resumable upload, and finalization.

The repository is currently dirty with unrelated migration work. New integration assets therefore use isolated new paths and do not edit or stage existing v2/legacy changes except for an additive root ignore boundary.

## Goals / Non-Goals

**Goals:**

- Make the asset identity, public trust root, build inputs, and publication commands reviewable in Ra3MapUtils.
- Keep private signing material and AssetCenter bearer credentials outside the repository.
- Make the same PowerShell scripts usable locally and from any CI runner.
- Produce a self-contained `win-x64` application bundle from `src/UI/UI.csproj`.
- Explain the later embedded SDK and Bootstrapper activation work without changing startup behavior in this change.

**Non-Goals:**

- Replacing Velopack in the legacy application.
- Modifying the v2 WPF startup or settings UI.
- Publishing a first application version before its product version is selected.
- Automatically signing or promoting a channel as an unreviewed side effect of building.
- Enabling telemetry before consent wording and UI choice are integrated.

## Decisions

### Use `cn.dreamness.ra3maputils` as the immutable asset ID

The lowercase reverse-domain identifier follows the existing organization namespace while preserving the exact English product name in display metadata. `Ra3MapUtils` is one independently versioned application and therefore one trust boundary.

Alternative: reuse the generic pilot asset. Rejected because it would share keys, releases, telemetry, and rollback authority across unrelated applications.

### Store only public integration assets in `eng/assetcenter`

Checked-in files include `asset.json`, the public key, scripts, templates, and operator notes. The private key defaults to `%LOCALAPPDATA%\AssetCenter\Ra3MapUtils\keys\ra3maputils.private.json`; CI supplies an equivalent protected file and sets `RA3MAPUTILS_ASSETCENTER_PRIVATE_KEY_PATH`. Generated publish/bundle state lives under `.artifacts/assetcenter` and is ignored.

Alternative: keep an encrypted private key in Git. Rejected because repository access and key-unlock access must remain separate, and history makes accidental disclosure difficult to revoke.

### Split build, release publication, and channel submission

`Build-AssetCenterBundle.ps1` publishes and verifies a bundle. `Publish-AssetCenterRelease.ps1` creates a draft, uploads, and finalizes it. `Submit-AssetCenterChannelPublication.ps1` accepts a separately reviewed canonical signed document. Each script has explicit inputs, uses terminating errors, and emits a concise JSON result suitable for CI.

Alternative: one script that builds and immediately promotes stable. Rejected because it collapses trust, test, and rollout gates and makes accidental stable publication too easy.

### Require explicit tool and version inputs

Scripts resolve Publisher and 7-Zip from parameters or dedicated environment variables. The version is a required parameter and is passed into MSBuild as `Version` and `InformationalVersion`; no Git metadata is consulted. The v2 UI project is published self-contained for `win-x64`, and `Ra3MapUtils.exe` is the signed entry point.

Alternative: call legacy `dev_tools/package.ps1`. Rejected because that script targets the legacy project, Velopack, a repository VERSION file, and local `vpk`/`7z` conventions.

### Keep embedded SDK guidance separate from this tooling change

The SDK can provide in-app checking, progress, policy hooks, and optional telemetry, while Bootstrapper/Installation owns side-by-side activation and health rollback. The guide documents both, but application code changes should be a later focused change after the v2 settings/update UX is ready and private SDK packages have a chosen distribution mechanism.

## Risks / Trade-offs

- [The asset ID is immutable after registration] → Record it in configuration and registration evidence before the first release; use a dedicated key.
- [The v2 source tree is currently uncommitted] → Add only isolated tooling/docs and validate without modifying unrelated migration files.
- [Publisher is not yet distributed as a standalone organization tool] → Require an explicit executable path and document a local AssetCenter build as the initial source; CI can later consume a private tool package.
- [Channel signing is not a Publisher CLI command] → Accept only an already signed canonical document and keep the operator signing step explicit rather than duplicating protocol cryptography in this repository.
- [Self-contained .NET 10 output is large] → Use AssetCenter content-addressed 7z blocks and measure the first real bundle before selecting a long-term profile.
- [User data could be overwritten if stored inside the version directory] → The integration guide requires configuration, logs, caches, and maps to remain outside immutable application versions.

## Migration Plan

1. Generate a dedicated key outside the repository and copy only the public document into `eng/assetcenter/keys`.
2. Register `cn.dreamness.ra3maputils`, create `beta` and `stable`, and issue an asset-scoped publisher/BI credential.
3. Validate scripts with a non-publishing bundle build from `src/UI`.
4. Select the first v2 application version, create/finalize a release, then submit a reviewed beta publication.
5. Implement the documented Bootstrapper/health integration in a subsequent change, run a local beta update/rollback, and only then promote stable.

Rollback for repository tooling is removal of the new `eng/assetcenter` and documentation files. Production metadata is append-only: a bad release is withdrawn or rolled back by a higher signed channel sequence; keys and credentials are revoked rather than deleted from history.
