# Ra3MapUtils 子模块 Agent 规范

## 1) 适用范围
- 本文件适用于 `Ra3MapUtils/` 目录及其子目录。
- 若与仓库根 `AGENTS.md` 冲突，以本文件为准。

## 2) 模块定位
- `Views/` + `ViewModels/`：WPF MVVM UI 层。
- `Services/Interface` + `Services/Impl`：服务抽象与实现。
- `API/`：内嵌 HTTP API 控制器（`ApiResponse<T>` 返回约定）。
- `MCP/`：MCP 工具（`[McpServerToolType]` / `[McpServerTool]`）。
- `data/plugins` + `data/nano_programs`：插件与微程序资源。
- `App.xaml.cs`：DI 注册、Web 服务启动、单实例控制关键入口。

## 3) 必须遵守的改动规则
- MVVM 改动：
  - 页面行为改动需同步检查对应 `View` 与 `ViewModel`。
  - 命令优先使用 `RelayCommand`；可观察属性优先 `ObservableProperty`。
- Service 改动：
  - 新能力先定义接口，再落实现；最后在 `App.ConfigureServices()` 注册。
  - 禁止在 ViewModel 直接跳过 Service 访问底层 DAO。
- API 改动：
  - 路由统一放在控制器 `[Route("api/...")]` 下。
  - 统一返回 `ApiResponse<T>`，失败分支要给出可诊断信息。
- MCP 改动：
  - 工具类必须保留 `[McpServerToolType]`。
  - 工具方法必须标注 `[McpServerTool]` 并补充说明。
- 插件/微程序改动：
  - 插件目录遵循 `plugin_meta.json` + `Main.cs` + `readme.txt`。
  - 微程序目录遵循 `info.json` + `Main.cs`。
  - 若涉及输出资源，保持 `.csproj` 中 CopyToOutput 规则一致。

## 4) 高风险区域（先确认后改）
- `App.xaml.cs`：
  - 单实例 `Mutex`、Web 启动（`127.0.0.1:30033`）、Swagger、MCP 映射（`/mcp`）。
- `Services/Impl/NewWorldBuilderPluginService.cs`：
  - 插件安装流程会复制/覆盖目标目录文件。
- `Utils/MapLuaImporterUtil.cs`：
  - 直接影响地图脚本内容与保存行为。

## 5) 快速验证清单
1. 结构验证：接口、实现、DI 注册、调用点是否齐全。  
2. 构建验证：`dotnet build Ra3MapUtils\Ra3MapUtils.csproj --no-restore`。  
3. 路由/工具验证（按需）：
   - `rg "\[Route\(|\[Http(Get|Post|Put|Delete)" API`
   - `rg "\[McpServerToolType\]|\[McpServerTool\]" MCP`
4. 若构建输出含 `NU190x`/`NU1701` 噪音，在结论中标注“是否新增”。

## 6) Do / Don't
- Do：
  - 保持 UI -> Service -> Business/DAO 调用链条清晰。
  - 修改端点时同步更新文档注释（XML 注释/说明文本）。
- Don't：
  - 不破坏 `App.xaml.cs` 启动顺序。
  - 不将 `data/nano_programs/*/Main.cs` 纳入主工程编译源文件。

