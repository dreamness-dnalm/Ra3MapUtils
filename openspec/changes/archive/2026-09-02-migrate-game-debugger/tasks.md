## 1. Bundle and foundation

- [x] 1.1 Copy Ra3Hacker data (`Injector`, `setting.json`, natives) into the v2 UI project with CopyToOutput rules
- [x] 1.2 Add `Ra3Hacker.Sdk` reference under `src/lib` (from legacy snapshot) for time/Lua clients
- [x] 1.3 Implement debugger settings load/save + injector launch service (no companion Injector mutex; no NWB deps)

## 2. Tool windows

- [x] 2.1 Add map-path settings window/ViewModel (folder, hide built-in, save, restart warning) with single-instance activate
- [x] 2.2 Add time-control window/ViewModel via Ra3Hacker API with clear offline errors; single-instance activate
- [x] 2.3 Add Lua executor window/ViewModel via Ra3Hacker API with clear offline errors; single-instance activate
- [x] 2.4 Wire “open debugger” launch action (missing-binary error path)

## 3. Toolbox catalog, icons, localization

- [x] 3.1 Register four toolbox catalog entries and launcher wiring (follow `ra3-toolbox-tool` for 512×512 icons)
- [x] 3.2 Add zh-CN / en strings for toolbox entries and window chrome/controls
- [x] 3.3 Register DI for windows/services

## 4. Verification

- [x] 4.1 Build v2 UI successfully with Ra3Hacker assets in output
- [x] 4.2 Verify: launch injector; save map settings; reopen map/time/Lua windows activates existing
- [x] 4.3 Verify: time/Lua report API-down clearly; no NWB/log-viewer/plugin surfaces added
