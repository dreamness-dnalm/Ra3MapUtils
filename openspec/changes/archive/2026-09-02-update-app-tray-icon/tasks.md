## 1. Prepare artwork

- [x] 1.1 Key black canvas outside the squircle to transparency from the supplied source image
- [x] 1.2 Export a high-resolution transparent PNG suitable for `Assets/icon.png`

## 2. Ship assets

- [x] 2.1 Build multi-size `app.ico` (at least 16/32/48/256) from the keyed art and replace `src/UI/Assets/app.ico`
- [x] 2.2 Replace `src/UI/Assets/icon.png` with the keyed PNG
- [x] 2.3 Confirm `UI.csproj` still copies both assets and `ApplicationIcon` still points at `app.ico` (no tray code change unless broken)

## 3. Verification

- [x] 3.1 Build v2 UI and confirm output `Assets/app.ico` / `icon.png` update
- [x] 3.2 Spot-check: no black corners; tray and exe use the shared ICO
