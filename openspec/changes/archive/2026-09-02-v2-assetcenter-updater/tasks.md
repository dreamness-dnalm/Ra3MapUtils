## 1. Version and SDK foundation

- [x] 1.1 Set companion product version to `2.0.0` in v2 build metadata and expose it to settings UI
- [x] 1.2 Add AssetCenter.Sdk dependency via private feed or non-absolute local package path; fail clearly if missing
- [x] 1.3 Implement companion-only update facade (stable channel, dedicated state directory under AppData) without coupling to future parallel AssetIds

## 2. Settings UX and activation

- [x] 2.1 Add settings update section: current version, check, status, download/progress, apply/restart guidance
- [x] 2.2 Wire download → verify/stage → schedule exit activation (no in-place overwrite of running exe)
- [x] 2.3 When Bootstrapper health env is present, confirm healthy start after core UI is ready
- [x] 2.4 Add zh-CN / en strings for all new update UI copy

## 3. Publish flow cleanup

- [x] 3.1 Document canonical `2.0.0` / stable operator path in `eng/assetcenter` README (build → publish → reviewed channel submit)
- [x] 3.2 Align script comments/templates with companion-only packaging and note future multi-asset extension point (no Lua pack implementation)
- [x] 3.3 Refresh `docs/assetcenter-sdk-integration.md` for stable-only v2 client + version `2.0.0` (defer Lua client details)

## 4. Verification

- [x] 4.1 Build v2 UI with SDK reference and version `2.0.0` visible in settings
- [x] 4.2 Verify check/up-to-date and check-failure UI paths; verify download/stage path against available tooling
- [x] 4.3 Confirm companion publish scripts still target `src/UI` only and do not bundle Lua library content
