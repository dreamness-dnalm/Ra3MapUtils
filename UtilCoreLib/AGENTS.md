# UtilCoreLib 子模块 Agent 规范

## 1) 适用范围
- 本文件适用于 `UtilCoreLib/` 目录。
- 本模块是地图底层工具库，很多方法有直接文件系统副作用。

## 2) 模块职责
- `mapFileHelper/`：地图目录识别、复制、重命名、删除、压缩、列举。
- `mapScriptHelper/`：脚本组与脚本构造辅助。
- `mapstrFileHelper/`：`map.str` 相关处理。
- `mapXmlOperator/`：地图 XML 数据处理。
- `utils/`：日志、文件、XML 辅助函数。

## 3) 必须遵守的改动规则
- 文件操作安全：
  - 涉及地图目录参数时，先走 `MapFileHelper.TranslateMapPath` 规范化。
  - 删除/移动/压缩前必须确认目标是合法地图目录（`.map` 核心文件存在）。
  - 对“批量操作地图文件”改动时必须显式考虑异常回滚或失败提示。
- 兼容性：
  - 本模块目标 `net45`，禁止引入高版本专属 API。
  - 保持对 `MapCoreLib` 相关类型与调用方式兼容。
- 副作用意识：
  - `Copy/Move/Del` 会真实改动地图目录，不允许“先删后验”式实现。
  - 变更日志输出时保持 `Logger` 语义稳定，避免吞掉关键错误信息。

## 4) 与上层耦合点
- `SharedFunctionLib` 与 `Ra3MapUtils` 都会调用本模块能力。
- 一旦改动路径解析或地图判定逻辑，会联动影响：
  - 地图管理页（复制/重命名/删除）
  - 插件联动与脚本导入相关流程

## 5) 快速验证清单
1. 构建验证：`dotnet build UtilCoreLib\UtilCoreLib.csproj --no-restore`  
2. 关键函数走读（至少）：
   - `TranslateMapPath`
   - `IsMap`
   - `Copy/Move/Del`
3. 风险复核：确认不存在“路径未规范化即执行文件操作”的新路径。

## 6) Do / Don't
- Do：
  - 优先做保守改动，确保地图文件安全。
  - 明确异常信息，便于上层 UI 提示用户。
- Don't：
  - 不在不校验地图合法性的情况下执行删除或覆盖。
  - 不把模块改造成依赖 UI 或主程序运行时状态。

