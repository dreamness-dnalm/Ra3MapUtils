# Feature Specification: Ra3MapUtils Current Architecture Baseline

**Feature Branch**: `[001-current-architecture-baseline]`  
**Created**: 2026-05-13  
**Status**: Accepted Baseline  
**Input**: User description: "为现有 Ra3MapUtils 项目引入 spec-kit / spec coding，先补全现状规格，再把后续功能纳入 spec-driven 流程。"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 主应用变更有清晰边界 (Priority: P1)

维护者在修改 WPF 页面、ViewModel、Service、内嵌 HTTP API、MCP 工具、插件或微程序能力前，可以先从规格中确认所属层级、调用链和高风险区域。

**Why this priority**: 主应用是用户直接入口，也是 DI、Web 服务、Swagger、MCP 和插件安装等关键启动链路所在。

**Independent Test**: 选择任一主应用改动需求，能够根据本规格判断应从 View/ViewModel、Service、API、MCP 或资源目录哪一层开始，并列出最小验证命令。

**Acceptance Scenarios**:

1. **Given** 一个涉及 ToolBoxPage 的新工具需求，**When** 编写 spec/plan，**Then** 规格必须描述 View/ViewModel、Service 或资源注册的改动边界。
2. **Given** 一个涉及 API 或 MCP 的新能力，**When** 编写 tasks，**Then** 任务必须包含接口声明、实现、注册/映射、调用点和文档注释。
3. **Given** 一个涉及插件或微程序资源的改动，**When** 进入实现前，**Then** 计划必须说明元数据结构、输出复制规则和覆盖风险。

---

### User Story 2 - 业务与持久化变更可兼容演进 (Priority: P1)

维护者在修改 SharedFunctionLib 的 Business、DAO、Models、Utils 时，可以按规格确认 .NET Framework 4.5 兼容、SQLite/linq2db 映射、配置键命名和升级路径。

**Why this priority**: 配置和元数据持久化影响用户历史数据，破坏性变更会直接造成启动或功能异常。

**Independent Test**: 选择一个新增配置项或表字段需求，能够写出 Settings/DAO/Model/SqliteConnection 的变更链和历史数据兼容策略。

**Acceptance Scenarios**:

1. **Given** 需要新增数据库表或列，**When** 编写 plan，**Then** 必须包含 `InitDB()`、`Upgrade()` 或等价升级路径。
2. **Given** 需要新增配置键，**When** 编写 spec，**Then** 必须使用模块前缀并说明历史键读取策略。
3. **Given** 需要新增依赖包，**When** 编写 plan，**Then** 必须说明该包可用于目标框架 `net45`。

---

### User Story 3 - 地图底层操作先处理副作用风险 (Priority: P1)

维护者在修改 UtilCoreLib 的地图目录、Lua、map.str、XML 或文件操作能力前，可以从规格中确认路径规范化、地图合法性校验和异常处理要求。

**Why this priority**: 底层工具会真实复制、移动、删除或改写地图文件，错误会造成用户资产损坏。

**Independent Test**: 选择一个地图批量操作需求，能够写出路径规范化、合法地图确认、失败提示和必要回滚策略。

**Acceptance Scenarios**:

1. **Given** 需要删除或移动地图目录，**When** 编写 tasks，**Then** 必须先列出 `TranslateMapPath` 和合法地图校验任务。
2. **Given** 需要修改 map.str 或 XML 处理，**When** 编写 plan，**Then** 必须说明文件编码、异常处理和上层提示方式。
3. **Given** 批量操作部分失败，**When** 编写 spec，**Then** 必须定义用户可诊断的失败结果。

---

### User Story 4 - 知识库与 CLI 边界可独立维护 (Priority: P2)

维护者在修改 KnowledgeBaseLib 或 KnowledgeBaseCli 时，可以从规格中确认 SQLite FTS、Jieba tokenizer、CLI 输入输出和主应用集成边界。

**Why this priority**: 知识库是独立模块，既服务主应用，也可能通过 CLI 维护数据。

**Independent Test**: 选择一个知识库索引或查询需求，能够判断改动在 KnowledgeBaseLib、KnowledgeBaseCli 或主应用页面中的位置。

**Acceptance Scenarios**:

1. **Given** 需要调整全文检索行为，**When** 编写 plan，**Then** 必须说明 FTS 表、分词器和既有数据兼容影响。
2. **Given** 需要调整 CLI 参数，**When** 编写 tasks，**Then** 必须包含 CLI 帮助、错误输出和库层调用边界。

---

### User Story 5 - 后续功能默认进入 spec-kit 流程 (Priority: P2)

维护者在新增功能或高风险修复时，能够使用 Codex spec-kit skills 生成中文主体 spec、plan、tasks，并在实现前完成一致性检查。

**Why this priority**: spec coding 的价值来自可重复流程，而不是一次性的文档初始化。

**Independent Test**: 使用 `$speckit-specify` 为一个小功能创建规格草案，并能引用本基线和 constitution 完成计划检查。

**Acceptance Scenarios**:

1. **Given** 一个新功能想法，**When** 启动 `$speckit-specify`，**Then** 输出应使用中文主体并保持路径/符号英文。
2. **Given** 规格存在不明确行为，**When** 进入计划前，**Then** 应使用 `$speckit-clarify` 或在 assumptions 中显式记录默认选择。
3. **Given** tasks 已生成，**When** 实现前，**Then** 应运行 `$speckit-analyze` 检查 spec/plan/tasks 一致性。

### Edge Cases

- 现有代码行为与文档不一致时，以代码和更具体目录 `AGENTS.md` 为事实来源，并在 spec assumptions 中标注不确定点。
- 修改 `App.xaml.cs`、插件安装、地图文件操作、DAO 升级等高风险路径时，必须先写风险、兼容性和验证方式。
- 构建输出出现 `NU190x` 或 `NU1701` 时，交付结论必须区分既有噪音和新增问题。
- 上游 spec-kit 模板为英文时，本项目产物使用中文主体，保留英文命令、路径、类型名和属性名。
- 无正式单元测试覆盖的模块，必须至少提供项目级构建、关键函数走读或手工验证说明。

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Repository MUST contain a spec-kit constitution that incorporates project layering, compatibility, persistence, map-file safety, UTF-8, validation, and governance rules.
- **FR-002**: Repository MUST contain a current architecture baseline spec covering `Ra3MapUtils`, `SharedFunctionLib`, `UtilCoreLib`, `KnowledgeBaseLib`, and `KnowledgeBaseCli`.
- **FR-003**: Future feature specs MUST use Chinese as the primary prose language while preserving English code identifiers, paths, commands, and protocol names.
- **FR-004**: Future plans MUST run a Constitution Check covering layering, target framework compatibility, persistence compatibility, side effects, high-risk paths, and minimum validation.
- **FR-005**: Future tasks MUST include exact repository paths, dependency ordering, parallelization markers where safe, and minimum validation commands.
- **FR-006**: API changes MUST preserve the controller route convention and `ApiResponse<T>` response pattern unless a spec explicitly defines and justifies a breaking change.
- **FR-007**: MCP tool changes MUST preserve `[McpServerToolType]` on tool classes and `[McpServerTool]` on tool methods.
- **FR-008**: DAO or schema changes MUST include an upgrade strategy and keep `SqliteConnection` mappings synchronized.
- **FR-009**: Map file operations MUST normalize map paths and validate legal map directories before destructive or overwriting work.
- **FR-010**: Plugin and nano-program changes MUST preserve their metadata file conventions and output resource behavior.
- **FR-011**: Validation reports MUST call out whether `NU190x` and `NU1701` warnings are existing noise or new issues.

### Key Entities *(include if feature involves data)*

- **Spec Artifact**: A spec-kit document under `specs/[###-feature]/` that records intent, technical plan, tasks, research, data model, contracts, and quickstart notes.
- **Application Layer**: A logical boundary such as View/ViewModel, Service, Business, DAO, Data, API, MCP, plugin resources, or map utility code.
- **High-Risk Change**: A change that touches startup, plugin installation, map file mutation, database schema/config, or script import behavior.
- **Validation Command**: The smallest command or manual check that provides confidence for the modified layer.
- **Compatibility Strategy**: The documented approach for preserving existing user data, target framework support, file formats, and public behavior.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new contributor can identify the correct layer and validation command for a proposed change in under 10 minutes using the baseline documents.
- **SC-002**: Every future high-risk feature plan includes explicit risk, compatibility, and validation sections before implementation starts.
- **SC-003**: Future tasks generated for this repository include concrete paths rather than generic `src/` placeholders.
- **SC-004**: The Codex spec-kit skills are available in the repository and can be used as `$speckit-*` commands.
- **SC-005**: The baseline introduces no runtime code changes and no required architecture rewrite.

## Assumptions

- This baseline documents current architecture and workflow; it does not change runtime behavior.
- The repository remains Windows-first because the main app is WPF and packaging scripts use PowerShell.
- `Ra3MapUtils`, `SharedFunctionLib`, and `UtilCoreLib` `AGENTS.md` files remain authoritative for their directories.
- Existing warning noise from package audit and legacy package compatibility may remain until a separate dependency modernization spec is created.
- Formal automated tests are not required as part of this baseline, but future feature specs may require tests when behavior or contracts change.
