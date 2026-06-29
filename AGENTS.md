# Ra3MapUtils Agent 协作规范（仓库级）

## 1) 适用范围与优先级
- 本文件适用于整个仓库：`Ra3MapUtils`。
- 子目录存在 `AGENTS.md` 时，子目录规则优先于本文件：
  - `Ra3MapUtils/AGENTS.md`
  - `SharedFunctionLib/AGENTS.md`
  - `UtilCoreLib/AGENTS.md`
- 若规则冲突，优先级：更具体目录 > 更上层目录。

## 2) 仓库职责地图
- 主干模块（生产功能）：
  - `Ra3MapUtils`：WPF 主程序（MVVM）、内嵌 HTTP API、MCP 工具、插件与微程序管理。
  - `SharedFunctionLib`：业务层与 DAO，配置/元数据持久化（SQLite + linq2db）。
  - `UtilCoreLib`：地图文件/脚本/map.str/XML 等底层工具。
  - `KnowledgeBaseLib`、`KnowledgeBaseCli`：知识库（SQLite FTS）与 CLI。
- 实验/兼容目录（默认非主改动路径）：
  - `test_field`：实验性样例与手工测试代码。
  - `WBLegacy`：legacy 兼容工程。
- 文档与打包：
  - `doc`：发布说明与截图。
  - `dev_tools`：打包/元数据脚本。

## 3) 通用工作流（必须）
1. 先定位改动层级：`View/ViewModel -> Service -> Business -> DAO -> 数据`。
2. 先读现有实现，再做最小改动；禁止跨层“顺手重构”。
3. 有接口改动时保持链路一致：接口声明、实现、DI 注册、调用点同步更新。
4. 先做最小验证，再补充必要说明（风险、兼容性、未验证项）。

## 4) 命令矩阵（以可执行为准）
- 推荐优先按项目构建，不直接依赖 `sln` 全量构建：
  - `dotnet build KnowledgeBaseLib\KnowledgeBaseLib.csproj`
  - `dotnet build KnowledgeBaseCli\KnowledgeBaseCli.csproj`
  - `dotnet build UtilCoreLib\UtilCoreLib.csproj --no-restore`
  - `dotnet build SharedFunctionLib\SharedFunctionLib.csproj --no-restore`
- 主程序构建：
  - `dotnet build Ra3MapUtils\Ra3MapUtils.csproj --no-restore`
  - 已知会出现较多 `NU190x`（漏洞审计）与 `NU1701`（兼容）噪音；请在结论中明确“是否为新引入问题”。
- 运行主程序：
  - `dotnet run --project Ra3MapUtils\Ra3MapUtils.csproj`
- 打包（需本机前置）：
  - `dev_tools\package.ps1`（依赖 `vpk`、`7z`）
  - `dev_tools\package_lualib.ps1`（依赖本机 Lua 库目录与 `7z`）

## 5) 编码与文本规则
- 仓库含大量中文文本，统一按 UTF-8 处理。
- PowerShell 读取文本时显式使用：`Get-Content -Encoding UTF8`。
- 文档与 JSON 修改后需确认无乱码、无错误转义。

## 6) Do / Don't
- Do：
  - 仅修改与当前任务直接相关的模块。
  - 保持模块边界清晰（UI 不直接写 DAO）。
  - 在提交说明中标注验证命令和结果。
- Don't：
  - 不在未确认影响面的情况下改动 `test_field` / `WBLegacy`。
  - 不把实验代码混入主流程目录。
  - 不在未说明风险时更改持久化表结构或关键路径（启动、插件安装、地图文件操作）。

## 7) 子模块入口
- 改主应用（页面、API、MCP、插件/微程序）前先读：`Ra3MapUtils/AGENTS.md`
- 改业务/DAO/配置存储前先读：`SharedFunctionLib/AGENTS.md`
- 改地图底层工具前先读：`UtilCoreLib/AGENTS.md`


<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->
