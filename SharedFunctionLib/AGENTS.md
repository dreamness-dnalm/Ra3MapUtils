# SharedFunctionLib 子模块 Agent 规范

## 1) 适用范围
- 本文件适用于 `SharedFunctionLib/` 目录。
- 本模块目标框架为 `.NET Framework 4.5`，兼容性优先。

## 2) 模块职责
- `Business/`：面向上层的业务门面（配置、插件、微程序元数据等）。
- `DAO/`：SQLite 持久化访问（linq2db + SQL）。
- `Models/`：持久化模型与轻量数据结构。
- `Utils/`：路径与连接封装（`Ra3MapUtilsPathUtil`、`SqliteConnection`）。

## 3) 必须遵守的改动规则
- 兼容性：
  - 禁止引入仅 `net6+/net8+` 才有的 API 或语法依赖。
  - 引用新增包前确认可用于 `net45`。
- DAO 规则：
  - DAO 入口方法保持 `InitDB()` 保护逻辑。
  - 增改表结构时必须提供升级路径（例如 `Upgrade()` 分支），不能只改建表 SQL。
  - 新增数据表/模型后，记得在 `Utils/SqliteConnection.cs` 中补充映射表属性。
- 配置键规则：
  - `SettingsDAO` 键名使用模块前缀（示例：`LuaImporter_*`、`Update_*`、`ApiService_*`）。
  - 改键名时必须考虑历史数据兼容读取。

## 4) 与主应用耦合点
- 路径与数据库：
  - `%AppData%/Ra3MapUtils` 下的 `Ra3MapUtils.db` 由 `Ra3MapUtilsPathUtil` 管理。
- 上层依赖：
  - `Ra3MapUtils` 中多个 Service 直接调用 `Business` 层静态方法。
  - 任何 `Business` 返回结构变更都可能影响 ViewModel/UI 展示。

## 5) 快速验证清单
1. 构建验证（建议增量）：`dotnet build SharedFunctionLib\SharedFunctionLib.csproj --no-restore`  
2. 依赖验证（如涉及）：`dotnet build UtilCoreLib\UtilCoreLib.csproj --no-restore`  
3. DAO 变更验证：
   - 检查 `InitDB/Upgrade` 是否覆盖新增列或索引。
   - 检查新增模型是否已接入 `SqliteConnection`。

## 6) Do / Don't
- Do：
  - 保持 `Business -> DAO -> DB` 分层职责单一。
  - 在数据库相关改动中记录兼容策略。
- Don't：
  - 不把 UI 逻辑塞进 `Business`/`DAO`。
  - 不做无升级策略的表结构破坏性变更。

