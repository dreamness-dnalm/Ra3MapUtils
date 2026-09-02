## Why

Ra3MapUtils needs a repeatable, provider-neutral path from its .NET 10 Windows build to AssetCenter so releases can be signed, uploaded, and consumed without GitHub Releases, tags, or Actions. The repository also needs an explicit SDK/updater onboarding guide and a safe key boundary before the first real asset registration.

## What Changes

- Define the immutable AssetCenter identity and public trust material for `Ra3MapUtils` while keeping its signing private key outside Git.
- Add PowerShell automation that publishes the v2 `src/UI` application for `win-x64`, creates and verifies an AssetCenter 7z bundle, and uploads/finalizes an explicitly versioned release.
- Keep the automation callable from a developer machine or arbitrary CI runner through parameters and environment-injected credentials; do not infer versions or channels from Git metadata.
- Add checked-in configuration/templates and defensive ignore rules for local credentials, private keys, generated bundles, and publish output.
- Add a Ra3MapUtils-specific SDK/updater integration guide covering trust configuration, beta/stable channels, installation layout, health confirmation, telemetry consent, and rollout.

## Capabilities

### New Capabilities

- `assetcenter-release-pipeline`: Provider-neutral, secret-safe build, packaging, publication, and integration guidance for the Ra3MapUtils Windows application.

### Modified Capabilities

None.

## Impact

- Adds repository tooling and documentation under `eng/assetcenter` and `docs` without changing legacy projects or the v2 application startup path.
- Uses the AssetCenter production control plane and `public-files.dreamness.cn` distribution origin.
- Requires .NET 10 for the application build, a separately supplied AssetCenter Publisher executable, a scoped publisher credential, and a signing private-key file outside the repository.
- Establishes a future integration point for `AssetCenter.Sdk`/Bootstrapper but does not silently replace the current application launcher or enable telemetry.
