## Why

v2 still ships the previous placeholder branding for the executable and system tray. A new map-companion icon is ready; the source art has black corners outside the squircle that must be keyed to transparency so taskbar and tray sizes do not show dirty edges.

## What Changes

- Replace `src/UI/Assets/app.ico` with a multi-size ICO built from the new artwork (transparent outside the squircle).
- Replace `src/UI/Assets/icon.png` with a matching high-resolution transparent PNG.
- Keep tray and application icons **shared** via existing `ApplicationIcon` + `SystemTrayService` loading of `Assets/app.ico` (no separate tray asset).
- Do **not** change legacy `Ra3MapUtils` project icons in this change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. Tray presence and close-to-tray behavior are unchanged; this is a branding asset refresh only (`skip_specs: true`).

## Impact

- `src/UI/Assets/app.ico`, `src/UI/Assets/icon.png` (and CopyToOutput / `ApplicationIcon` wiring already in `UI.csproj`).
- Visual only for exe, taskbar, and NotifyIcon; no API or settings changes.
