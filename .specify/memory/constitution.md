# Ra3MapUtils Constitution

## Core Principles

### I. 分层边界优先
所有功能必须先定位所属层级，再做最小必要改动。默认链路为 `View/ViewModel -> Service -> Business -> DAO -> 数据`。UI 层不得直接写 DAO 或绕过 Service 访问持久化；Service 新能力必须先定义接口，再补实现、DI 注册与调用点。跨层改动必须在 spec/plan 中说明调用链、兼容影响与验证方式。

### II. 兼容性与持久化安全
`Ra3MapUtils` 主程序以 .NET 8 WPF 为主，`SharedFunctionLib` 与 `UtilCoreLib` 保持 .NET Framework 4.5 兼容。新增 API、语法或包引用前必须确认目标框架可用。数据库表结构、配置键、元数据模型变更必须描述历史数据兼容策略、升级路径和回滚风险；禁止只改建表 SQL 而不处理既有数据库升级。

### III. 地图文件与扩展副作用安全
地图文件、Lua/map.str/XML、插件安装、微程序执行属于高风险路径。涉及地图目录的操作必须先规范化路径并确认合法地图结构；删除、移动、覆盖、压缩、批量处理必须在计划中写明失败提示、异常处理和必要的回滚策略。插件与微程序资源改动必须保持元数据结构和输出复制规则一致。

### IV. Spec-First 可追踪开发
新增功能、行为变更和高风险修复必须先形成 spec，再进入 plan/tasks/implement。规格使用中文主体，代码标识符、路径、命令保持英文。每个 task 必须包含具体路径、依赖顺序、可并行标记和最小验证命令；需求不清时先使用 `$speckit-clarify` 收敛，而不是直接编码。

### V. 最小验证与风险显式化
验证范围应与改动范围匹配，优先项目级构建而非全量 solution 构建。主程序默认验证命令为 `dotnet build Ra3MapUtils\Ra3MapUtils.csproj --no-restore`；底层库按涉及范围分别构建。若输出包含既有 `NU190x` 或 `NU1701` 噪音，结论必须明确是否为本次新增问题。未验证项必须在交付说明中显式列出。

## Project Constraints

本仓库包含 WPF 主应用、业务/DAO 库、地图底层工具、知识库与 CLI。默认不把 `test_field` 和 `WBLegacy` 作为主改动路径，除非 spec 明确说明实验或兼容目标。生产功能应优先落在 `Ra3MapUtils`、`SharedFunctionLib`、`UtilCoreLib`、`KnowledgeBaseLib` 或 `KnowledgeBaseCli` 的既有边界内。

仓库包含大量中文文本，文档、JSON 与配置统一按 UTF-8 处理。PowerShell 读取文本时使用 `Get-Content -Encoding UTF8`。修改中文文档或 JSON 后，交付说明必须说明是否已检查乱码和转义风险。

高风险区域包括 `Ra3MapUtils/App.xaml.cs`、`Ra3MapUtils/Services/Impl/NewWorldBuilderPluginService.cs`、`Ra3MapUtils/Utils/MapLuaImporterUtil.cs`、`SharedFunctionLib/DAO`、`SharedFunctionLib/Utils/SqliteConnection.cs`、`UtilCoreLib/mapFileHelper`、`UtilCoreLib/mapstrFileHelper`、`UtilCoreLib/mapXmlOperator`。触及这些路径时，spec/plan 必须包含风险、兼容性和验证方式。

## Development Workflow

新功能默认流程为 `$speckit-specify` -> `$speckit-clarify` -> `$speckit-plan` -> `$speckit-tasks` -> `$speckit-analyze` -> `$speckit-implement`。小型、低风险文档修复可跳过 clarify/analyze，但仍需保留清晰的 intent、路径和验证说明。

计划阶段必须执行 Constitution Check：确认分层边界、目标框架、持久化兼容、地图/插件副作用、中文/UTF-8、最小验证命令。若违反任一原则，必须在 Complexity Tracking 中写明原因和被拒绝的简单替代方案。

任务阶段必须按可独立验证的用户故事组织。涉及接口变更时，同步列出接口声明、实现、DI 注册、调用点、API/MCP 文档注释和验证命令。涉及数据迁移时，同步列出模型、DAO、升级路径和历史数据兼容检查。

## Governance

本 Constitution 补充 spec-kit 工作流治理；运行时协作规则仍遵循仓库与子目录 `AGENTS.md`。若存在冲突，优先级为更具体目录 `AGENTS.md` > 仓库根 `AGENTS.md` > 本 Constitution 中的通用流程描述。后续修改本 Constitution 必须同步更新版本、日期，并在变更说明中写明迁移影响。

所有 spec、plan、tasks 和实现交付都必须能追溯到本 Constitution 的原则。高风险改动没有风险说明、兼容策略或最小验证命令时，不得进入实现阶段。若因环境限制无法验证，必须在最终说明中记录限制和建议的后续验证。

**Version**: 1.0.0 | **Ratified**: 2026-05-13 | **Last Amended**: 2026-05-13
