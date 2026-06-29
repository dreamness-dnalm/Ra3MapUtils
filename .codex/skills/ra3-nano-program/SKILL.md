---
name: ra3-nano-program
description: Ra3MapUtils nano program workflow. Use when Codex adds, updates, debugs, or packages C# nano programs under Ra3MapUtils/data/nano_programs, including info.json metadata, Main.cs scripts, MapFilePath handling, Ra3MapFacade usage, and Ra3MapUtils.csproj CopyToOutput rules.
---

# RA3 Nano Program

## Workflow

Use this skill whenever a Ra3MapUtils nano program is added or changed.

1. Read the project rules first:
   - `AGENTS.md`
   - `Ra3MapUtils/AGENTS.md`

2. Inspect nearby examples before editing:
   - Official nano programs: `Ra3MapUtils/data/nano_programs/*/info.json`
   - Script bodies: `Ra3MapUtils/data/nano_programs/*/Main.cs`
   - Runtime loader: `Ra3MapUtils/Services/Impl/NanoProgramService.cs`
   - Output rules: `Ra3MapUtils/Ra3MapUtils.csproj`

3. Create or update exactly one nano program directory unless the user asks for more:

```text
Ra3MapUtils/data/nano_programs/YOUR_PROGRAM_ID/
├── info.json
└── Main.cs
```

Use a stable uppercase or descriptive directory name. Generate a new GUID for `info.json` when creating a new program.

## Metadata

Use UTF-8 JSON with this shape:

```json
{
    "ID": "new-guid-here",
    "Name": "显示名称",
    "Description": "一句话说明这个微程序做什么。",
    "Author": "dreamness"
}
```

Only change `Author` when the user gives a different value or an existing program already uses a different owner.

## Main.cs Patterns

Nano programs are C# scripts executed at runtime, not normal project source files. Keep scripts self-contained and copy patterns from existing programs.

For map-file programs, support `MapFilePath` first and fall back to the selector:

```csharp
using Ra3MapUtils.ScriptViews;
using System;
using System.Windows.Forms;
using Dreamness.Ra3.Map.Facade.Core;
using Ra3MapUtils.Utils;

string? mapFilePath = null;

if (ArgumentDictionary.TryGetValue("MapFilePath", out var argumentMapFilePath)
    && !string.IsNullOrWhiteSpace(argumentMapFilePath))
{
    mapFilePath = argumentMapFilePath;
}
else
{
    var dialog = MapFileSelectorDialog.Create("选择地图文件");
    dialog.ShowDialog();
    if (dialog.DialogResult == DialogResult.OK)
    {
        mapFilePath = dialog.Tag as string;
    }
}

if (string.IsNullOrWhiteSpace(mapFilePath))
{
    throw new Exception("未选择地图文件，程序终止。");
}

var ra3map = Ra3MapFacade.Open(mapFilePath);
```

For map mutations:

- Use `ra3map.Backup()` before `ra3map.Save()` and show the backup path.
- Include `using Ra3MapUtils.Utils;` when using `Backup()`; it is an extension method, not a `Ra3MapFacade` member.
- Use `MsgDialog.Create(...).ShowDialog()` for completion or no-op summaries.
- Keep no-op cases non-destructive: do not save when nothing changed.
- Do not add service/API/MCP/UI changes unless the user explicitly requests runtime integration beyond a nano program.

When facade behavior is unclear, inspect the C# library source at `N:\workspace\ra3\Ra3MapSharp`, especially:

- `src/Dreamness.RA3.Map.Facade/Core/Ra3MapFacade/`
- `src/Dreamness.RA3.Map.Parser/Asset/Impl/`

## Project File Rules

Update `Ra3MapUtils/Ra3MapUtils.csproj` so the script is copied but not compiled into the WPF app:

```xml
<None Update="data\nano_programs\YOUR_PROGRAM_ID\info.json">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
<Compile Remove="data\nano_programs\YOUR_PROGRAM_ID\Main.cs" />
<None Include="data\nano_programs\YOUR_PROGRAM_ID\Main.cs">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

Mirror existing item ordering when possible. Avoid reorganizing unrelated `ItemGroup` entries in a dirty worktree.

## Validation

Run the main project build after changes:

```powershell
dotnet build Ra3MapUtils\Ra3MapUtils.csproj --no-restore
```

Report whether warnings are pre-existing noise such as `NU190x`, `NU1701`, `NU1702`, or MVVM Toolkit warnings.

Also verify output copying when relevant:

```powershell
Test-Path Ra3MapUtils\bin\Debug\net8.0-windows\data\nano_programs\YOUR_PROGRAM_ID\Main.cs
Test-Path Ra3MapUtils\bin\Debug\net8.0-windows\data\nano_programs\YOUR_PROGRAM_ID\info.json
```

Remember: `dotnet build` does not compile nano program scripts. If the app reports a script compile error, fix the script usings/API calls and rebuild so the output copy is refreshed.
