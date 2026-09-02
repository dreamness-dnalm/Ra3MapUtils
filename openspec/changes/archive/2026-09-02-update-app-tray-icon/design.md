## Context

See proposal.md. v2 already points `ApplicationIcon` and `SystemTrayService` at `Assets/app.ico`; `icon.png` is copied to output. Source art is a blue squircle map icon with black canvas outside the rounded square.

## Goals / Non-Goals

**Goals:**
- Produce transparent-outside-squircle PNG and multi-size ICO from the provided source.
- Replace `app.ico` and `icon.png` in place; keep shared tray/app path.

**Non-Goals:**
- Separate tray-only artwork.
- Code changes to tray/window wiring unless a path or format issue appears.
- Legacy project icon refresh.
- Bootstrapper/installer branding packages.

## Decisions

### Decision: Key black corners to alpha, keep the blue squircle
Treat near-black pixels outside the icon body as transparent (flood/threshold from corners). Do not flatten to a full-bleed square that removes the intentional rounded blue plate unless transparency fails quality checks.

**Alternatives considered:** Crop to opaque bounding box only. Rejected—would leave hard square edges vs the designed squircle.

### Decision: One multi-size ICO for exe + tray
Embed at least 16, 32, 48, and 256 px. Tray continues to load `Assets/app.ico`.

**Alternatives considered:** Dedicated `tray.ico`. Deferred unless 16px readability fails after visual check.

### Decision: Tooling for conversion
Use available local tools (e.g. Magick.NET already in the solution, or Python/Pillow, or ImageMagick CLI) during apply to generate PNG/ICO; commit the resulting binary assets, not a mandatory runtime dependency for users.

## Risks / Trade-offs

- [Aggressive black threshold eats map shadow] → Constrain keying to corner-connected regions / low-luminance only.
- [JPEG source artifacts on edges] → Prefer working from the user-supplied file; soften fringe after keying if needed.
- [Windows caches old icons] → Note rebuild/clean output; users may need to refresh shortcuts after install.

## Migration Plan

1. Key transparency → write `icon.png`.
2. Build `app.ico` multi-size from that PNG.
3. Overwrite `src/UI/Assets/*`; rebuild UI and spot-check exe + tray.

## Open Questions

None — product choices locked (shared icon, key black corners, update both assets).
