# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Ra3MapUtils (RA3地编伴侣) is a Windows desktop application built with WPF (.NET 8.0) to assist with Command & Conquer: Red Alert 3 map editing. It provides map management, Lua script importing, auto-updates, and integrates with the "NewWorldBuilder" map editor through plugins and nano-programs.

## Build & Development Commands

### Build the Solution
```powershell
# Build in Debug mode
dotnet build Ra3MapUtils.sln

# Build in Release mode (x64)
dotnet build Ra3MapUtils.sln -c Release
```

### Run the Application
```powershell
# Run from the main project directory
dotnet run --project Ra3MapUtils/Ra3MapUtils.csproj
```
 
### Package for Release
```powershell
# Package using Velopack (requires vpk CLI and 7z)
.\dev_tools\package.ps1
# Output: dev_tools\publish\v{version}\Ra3MapUtils-v{version}.7z
```

### Package Lua Library
```powershell
.\dev_tools\package_lualib.ps1
```

### Restore Dependencies
```powershell
dotnet restore Ra3MapUtils.sln
```

## Solution Architecture

### Project Structure

```
Ra3MapUtils/              # Main WPF application (NET8.0-Windows)
├── Views/                # XAML UI components
├── ViewModels/           # MVVM ViewModels (CommunityToolkit.Mvvm)
├── Services/             # Business services with DI
├── Models/               # Data models
├── API/                  # RESTful API controllers (embedded ASP.NET Core)
├── MCP/                  # Model Context Protocol tools for AI integration
├── Utils/                # Utilities (converters, validators, path helpers)
├── data/                 # Application data files
│   ├── plugins/          # NewWorldBuilder plugins
│   ├── nano_programs/    # C# script-based extensions
│   └── plugins_lib/      # Shared libraries for plugins
└── lib/                  # External DLLs (Dreamness.RA3.Map.*)

SharedFunctionLib/        # Shared business logic (NET4.5)
├── Business/             # Business layer (LuaImporter, Update, etc.)
├── DAO/                  # Data access layer (SQLite)
└── Models/               # Shared data models

UtilCoreLib/              # RA3 map utilities (NET4.5)
├── mapFileHelper/        # Map file operations
├── mapScriptHelper/      # Lua script helpers
├── mapstrFileHelper/     # Map string operations
└── mapXmlOperator/       # XML parsing

KnowledgeBaseLib/         # Full-text search database (NET8.0)
└── DAO/                  # SQLite FTS5 with Jieba tokenizer

KnowledgeBaseCli/         # CLI for knowledge base (NET8.0)
test_field/               # Testing playground
```

### Dependency Graph

```
Ra3MapUtils
├── KnowledgeBaseLib
├── SharedFunctionLib
│   └── UtilCoreLib
└── UtilCoreLib

KnowledgeBaseCli
└── KnowledgeBaseLib
```

### Key Architectural Patterns

**MVVM Pattern**: Uses `CommunityToolkit.Mvvm` with:
- `ObservableObject` base class for ViewModels
- `[ObservableProperty]` for bindable properties
- `RelayCommand` for commands
- `WeakReferenceMessenger` for inter-component messaging

**Dependency Injection**: Configured in `App.xaml.cs`:
- Services registered in `ConfigureServices()` method
- Uses Microsoft.Extensions.DependencyInjection
- Singletons for pages, view models, and most services

**Layered Architecture**:
```
UI (XAML + ViewModels)
  ↓
Services Layer (Ra3MapUtils/Services/Impl/)
  ↓
Business Layer (SharedFunctionLib/Business/)
  ↓
DAO Layer (SharedFunctionLib/DAO/)
  ↓
SQLite Database
```

**Embedded Web Server**:
- ASP.NET Core hosted on port 30033
- Provides RESTful APIs and Swagger UI
- MCP (Model Context Protocol) server for AI integration
- Started in `App.xaml.cs` using `WebApplication.CreateBuilder()`

## Extensibility Systems

### 1. NewWorldBuilder Plugins

Plugins extend the NewWorldBuilder map editor with custom functionality. They are managed through `INewWorldBuilderPluginService`.

**Plugin Structure**:
```
data/plugins/RA3MapUtil_LuaImporter/
├── plugin_meta.json      # Metadata (name, version, file mappings)
├── Main.cs               # Plugin implementation
└── readme.txt            # Documentation
```

**Plugin Metadata** (`plugin_meta.json`):
```json
{
  "PluginName": "RA3MapUtil_LuaImporter",
  "PluginVersion": "v1.1.4",
  "RequireFileDictionary": {
    "source_file.cs": "target_file.cs"
  }
}
```

**Installation**: Plugins are auto-installed to NewWorldBuilder's script directory when detected.

### 2. Nano-Programs

Nano-programs are C# script-based micro-utilities that extend Ra3MapUtils functionality. They can be executed from the UI or via API.

**Locations**:
- Built-in: `Ra3MapUtils/data/nano_programs/`
- User: `%AppData%/Ra3MapUtils/nano_programs/user/`
- Store: `%AppData%/Ra3MapUtils/nano_programs/store/`

**Nano-Program Structure**:
```
UUID_Generator/
├── info.json             # Metadata (ID, Name, Description, Author)
└── Main.cs               # Executable C# code with Run() entry point
```

**Metadata** (`info.json`):
```json
{
  "ID": "uuid-generator",
  "Name": "UUID Generator",
  "Description": "Generates UUIDs for map objects",
  "Author": "dreamness"
}
```

**Service**: Managed by `INanoProgramService` - handles discovery, execution order, enable/disable state.

**Execution**: Uses `Dreamness.ScriptExecutor` to compile and run C# scripts with full assembly access.

### 3. MCP (Model Context Protocol) Tools

MCP tools expose application functionality to AI assistants like Claude.

**Location**: `Ra3MapUtils/MCP/`

**Available Tools**:
- `CSharpScriptService.RunRa3CSharpScript()` - Execute C# scripts
- `CSharpScriptService.GetLibDocument()` - Get library documentation
- `CSharpScriptService.GetLibStructure()` - Get assembly structure
- `NanoProgramMCP.GetAllNanoProgramsInfo()` - List nano-programs
- `NanoProgramMCP.RunNanoProgram()` - Execute nano-program

**Decoration**: Use `[McpServerToolType]` on classes and `[McpServerTool]` on methods.

### 4. RESTful API

**Base URL**: `http://localhost:30033`
**Swagger UI**: `http://localhost:30033/swagger`

**Controllers**:
- `CSharpScriptAPIService` - `/csharp-script/*` - Execute C# scripts
- `NanoProgramAPIService` - `/nano-program/*` - Nano-program operations
- `StatusAPIService` - `/status` - Application status

## Important Implementation Details

### Data Persistence

**Settings Database**: `%AppData%/Ra3MapUtils/Ra3MapUtils.db`
- Engine: SQLite with linq2db ORM
- Stores: Application settings, Lua library configs, nano-program metadata
- DAO classes in `SharedFunctionLib/DAO/`

**Knowledge Base**: `%AppData%/Ra3MapUtils/knowledge_base/knowledge_base.db`
- Engine: SQLite FTS5 with Jieba tokenizer (Chinese full-text search)
- DAO: `KnowledgeBaseLib/DAO/KnowledgeBaseDAO.cs`

### External Dependencies

**Dreamness.RA3.Map Libraries** (in `Ra3MapUtils/lib/`):
- `Dreamness.RA3.Map.Facade.dll` - High-level map operations
- `Dreamness.RA3.Map.Parser.dll` - RA3 map file parsing
- `Dreamness.RA3.Map.Transform.dll` - Map transformations
- `Dreamness.Ra3.Map.Visualization.dll` - Map visualization
- `Dreamness.RA3.Map.Lua.dll` - Lua script operations
- `Dreamness.ScriptExecutor.dll` - C# script execution engine

These are proprietary libraries specific to RA3 map manipulation and are NOT available on NuGet.

### Single Instance Pattern

The application prevents multiple instances using a Mutex in `App.xaml.cs`:
```csharp
Mutex mutex = new(true, "Ra3MapUtils", out bool createdNew);
if (!createdNew) {
    // Bring existing instance to foreground
    return;
}
```

### Auto-Update System

Uses **Velopack** for delta updates:
- Version file: `Ra3MapUtils/VERSION`
- Update service: `IUpdateService`
- Package script: `dev_tools/package.ps1`

### Lua Library Management

**Service**: `ILuaImportService`
- Auto-loads Lua libraries from plugins and user directories
- Binds libraries to RA3 map scripts
- Stores configuration in `LuaLibConfigDAO`

### UI Framework

**WPF-UI 3.0.5** (Fluent Design):
- Modern Windows 11-style UI
- Uses `FluentWindow` as base for windows
- Theme support with `ThemeManager`

**HandyControl 3.6.0-rc2**:
- Additional UI controls and helpers
- Chinese localization support

**AvalonEdit 6.3.0**:
- Code editor with Lua syntax highlighting
- Custom `.xshd` file: `Ra3MapUtils/lua4.xshd`

## Version Management

Version is stored in `Ra3MapUtils/VERSION` as plain text (e.g., `1.8.2`). This file is:
- Embedded as a resource in the project
- Read by the package script for release naming
- Used by the auto-update system

## Common Navigation Paths

**Main Pages** (8 pages accessible from navigation):
1. `HomePage` - Welcome and quick actions
2. `MapManagePage` - Map file management (copy, rename, delete)
3. `ToolBoxPage` - Utility scripts and tools
4. `KnowledgeBasePage` - Full-text search documentation
5. `ScriptListPage` - Nano-program management
6. `AIPage` - AI-assisted editing with MCP
7. `SettingPage` - Configuration (update, paths, Lua libs)
8. `AboutPage` - Version and credits

**Sub-Windows** (modal dialogs):
- `LuaManagerWindow` - Lua library import and configuration
- `BorderManagerWindow` - Map border editing
- `CodeEditorWindow` - Script editing with AvalonEdit
- `LogViewerWindow` - Application log viewer
- `ChatLuaHelperWindow` - Lua script generation assistant
- `TerrainTransWindow` - Terrain transformation tools

## Target Framework Notes

**Multi-targeting**:
- Main app: NET8.0-Windows (WPF)
- Legacy libraries: NET4.5 (SharedFunctionLib, UtilCoreLib) for compatibility with older tools
- Modern libraries: NET8.0 (KnowledgeBaseLib, KnowledgeBaseCli)

**Platform**: Windows-only (x64 in Release builds)

## Testing

`test_field` project serves as a playground for experimentation and ad-hoc testing. No formal unit test framework is currently in use.
