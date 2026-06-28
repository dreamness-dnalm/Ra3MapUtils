---
name: ra3-toolbox-tool
description: Ra3MapUtils toolbox tool workflow. Use when Codex adds or modifies tools in the WPF ToolBoxPage or toolbox page, creates toolbox subwindows or ViewModels, or adds CardAction entries; automatically create a matching 512x512 entry icon in Ra3MapUtils/data/imgs, register it as a Resource in Ra3MapUtils.csproj, and wire the ToolBoxPage image source.
---

# RA3 Toolbox Tool

## Workflow

Use this skill whenever a Ra3MapUtils toolbox tool is added or its toolbox entry is changed.

1. Follow the existing toolbox structure:
   - Entry cards: `Ra3MapUtils/Views/MainWindowPages/ToolBoxPage.xaml`
   - Entry commands: `Ra3MapUtils/ViewModels/MainWindowPages/ToolBoxPageViewModel.cs`
   - Tool windows: `Ra3MapUtils/Views/SubWindows/toolbox/`
   - Tool view models: `Ra3MapUtils/ViewModels/SubWindows/toolbox/`
   - DI registrations: `Ra3MapUtils/App.xaml.cs`

2. For every new toolbox entry, create a dedicated icon. Do not leave the generic `data/imgs/icon.png` placeholder.

3. Match the existing toolbox icon style:
   - 512x512 transparent PNG.
   - Blue circular badge background.
   - Dark blue outline and simple light foreground panel.
   - Optional yellow corner badge for the tool's key concept.
   - Name the file with the tool name, for example `image_encoding_tool_icon.png`.

4. Put the icon in `Ra3MapUtils/data/imgs/`, then register it in `Ra3MapUtils/Ra3MapUtils.csproj`:

```xml
<None Remove="data\imgs\your_tool_icon.png" />
<Resource Include="data\imgs\your_tool_icon.png" />
```

5. Wire the toolbox card image with a pack URI:

```xml
Source="pack://application:,,,/data/imgs/your_tool_icon.png"
```

6. Build after changes:

```powershell
dotnet build Ra3MapUtils\Ra3MapUtils.csproj --no-restore -m:1 -p:NoWarn=NU1901%3BNU1902%3BNU1903%3BNU1701
```

## Icon Script

Use `scripts/create_toolbox_icon.py` for deterministic first-pass icons:

```powershell
python .codex\skills\ra3-toolbox-tool\scripts\create_toolbox_icon.py --output Ra3MapUtils\data\imgs\image_encoding_tool_icon.png --symbol image --badge 64
```

Arguments:

- `--output`: Required output PNG path, absolute or relative to the repo root.
- `--symbol`: One of `image`, `code`, `table`, `window`, or `tool`.
- `--badge`: Optional 1-3 character badge text, such as `64`, `JS`, `Lua`, or `AI`.

If the generated icon is close but not ideal, adjust the script parameters or patch the script locally for the specific tool. Preserve the shared visual language above.

## Final Check

Before finishing a toolbox-tool task, verify:

- The toolbox entry uses a dedicated icon file.
- The icon is included as a WPF `Resource`.
- The card image source points at the new `data/imgs/*.png` file.
- The build succeeds, allowing for pre-existing warnings.
