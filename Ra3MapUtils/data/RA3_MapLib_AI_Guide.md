# RA3 C# 地图库（Dreamness.RA3.Map.Facade）  
# **AI 使用指南（Markdown 完整版）**

> 本文档根据作者 dreamness 提供的官方资料整理，结合 AI 使用需求重新组织结构。  
> 适用于 GPT 类模型、自动脚本生成器、MCP 工具等自动化开发场景。

---

# 目录
- [1. 基础概念](#1-基础概念)  
  - [1.1 地图尺寸](#11-地图尺寸)  
  - [1.2 坐标系统](#12-坐标系统)  
  - [1.3 对象类型](#13-对象类型)
- [2. 创建 / 打开 / 保存](#2-创建--打开--保存)
- [3. 地形（高度 / 通行性 / 纹理）](#3-地形高度--通行性--纹理)  
- [4. 玩家（Player）](#4-玩家player)  
- [5. 队伍（Team）](#5-队伍team)  
- [6. 单位（Unit）](#6-单位unit)  
- [7. 引用方式（NuGet）](#7-引用方式nuget)  
- [8. AI 使用最佳实践](#8-ai-使用最佳实践)  
- [9. 完整示例](#9-完整示例)

---

# 1. 基础概念

## 1.1 地图尺寸

创建地图时会设定：

- `playableWidth`
- `playableHeight`
- `border`（边界宽度）

实际地图大小计算：

```
width = playableWidth + 2 * border
height = playableHeight + 2 * border
```

示例：

可游玩区域 `200 x 300` ，边界 `20` → 实际地图 `240 x 340`

---

## 1.2 坐标系统

RA3 地图有两种坐标：

### ✔ 网格坐标（Grid）

用于地形（高度/通行性/纹理）
- 单位：整数
- 原点：左下角
- 示例：`(20, 30)`

### ✔ 世界坐标（World）

用于单位、路径点
- 单位：浮点
- 示例：`(100.0, 200.0)`

---

## 1.3 对象类型

- 玩家（Player）
- 队伍（Team）
- 单位（UnitObject）
- 路径点（Waypoint）
- 地形（Terrain）
- 纹理（TileTexture）

均使用 Add / Get / Remove 模式管理。

---

# 2. 创建 / 打开 / 保存

## 2.1 创建地图

```csharp
var ra3map = Ra3MapFacade.NewMap(200, 300, 10);
var ra3map = Ra3MapFacade.NewMap(200, 300, 10, 0);
var ra3map = Ra3MapFacade.NewMap(200, 300, 10, 2, "Grass_Geneva01");
```

---

## 2.2 打开地图

```csharp
var ra3map = Ra3MapFacade.Open(Ra3PathUtil.RA3MapFolder, "test_map");
```

---

## 2.3 保存地图

```csharp
ra3map.Save();
ra3map.SaveAs(Ra3PathUtil.RA3MapFolder, "new_map");
```

> 新建地图首次必须使用 SaveAs()

---

# 3. 地形（高度 / 通行性 / 纹理）

## 3.1 基本属性

```csharp
var width = ra3map.MapWidth;
var height = ra3map.MapHeight;
var playableWidth = ra3map.MapPlayableWidth;
var playableHeight = ra3map.MapPlayableHeight;
var borderWidth = ra3map.MapBorderWidth;
```

---

## 3.2 高度（Grid 坐标）

```csharp
var h = ra3map.GetTerrainHeight(10, 20);
ra3map.SetTerrainHeight(30, 40, 220);
```

---

## 3.3 通行性 Passability

可设置：

- Passable
- Impassable
- ImpassableToPlayers
- ImpassableToAirUnits
- ExtraPassable

```csharp
ra3map.UpdatePassabilityMap();
var pass = ra3map.GetPassability(10, 20);
ra3map.SetPassability(10, 20, "Passable");
```

---

## 3.4 纹理 TileTexture

```csharp
var textureName = ra3map.GetTileTexture(10, 20);
ra3map.SetTileTexture(10, 20, "Dirt_Yucatan03");
```

纹理图鉴：https://zybdatasupport.online/terrain

---

# 4. 玩家 Player

## 4.1 Add / Get / Remove

```csharp
var p1 = ra3map.AddPlayer("Player_1");
var player = ra3map.GetPlayer("Player_1");
var all = ra3map.GetPlayers();
ra3map.Remove(player);
```

## 4.2 字段

| 字段 | 含义 |
|------|------|
| Name | 玩家ID |
| DisplayName | 展示名称 |
| Color | 颜色 |
| RadarColor | 雷达颜色 |
| Faction | 阵营 |
| IsHuman | 是否为玩家控制 |

---

# 5. 队伍 Team

## 5.1 Add / Get / Remove

```csharp
var team = ra3map.AddTeam("team1", "Player_1");
var teams = ra3map.GetTeams();
ra3map.Remove(team);
```

## 5.2 常见字段

| 字段 | 含义 |
|------|------|
| Name | 队伍名 |
| OwnerPlayerName | 所属玩家 |
| FullName | 形式如 Player_1/team1 |
| AiType | AI 行为 |
| InitialAggressiveness | 初始攻击性 |

---

# 6. 单位 Unit

## 6.1 添加单位（World 坐标）

```csharp
var unit = ra3map.AddUnitObject("SovietSurveyor", 100, 200);
unit.ObjName = "u1";
unit.Angle = 30;
unit.BelongToTeam = "Player_1/team1";
```

## 6.2 获取与删除

```csharp
var units = ra3map.GetUnitObjects();
ra3map.Remove(units[0]);
```

## 6.3 字段（部分）

| 字段 | 含义 |
|------|------|
| ObjName | 对象名称 |
| TypeName | 单位类型 |
| Angle | 朝向 |
| BelongToTeam | 所属队伍 |
| Indestructible | 无敌 |
| Targetable | 可被攻击 |

---

# 7. 引用 NuGet

```bash
dotnet add package Dreamness.RA3.Map.Facade
```

---

# 8. AI 使用最佳实践

## 8.1 推荐流程

1. 创建或打开地图  
2. 读取宽高  
3. 修改高度  
4. 更新通行性  
5. 设置纹理  
6. 创建玩家 / 队伍  
7. 添加单位  
8. 保存  

---

## 8.2 坐标注意事项

- 地形：Grid 坐标  
- 单位：World 坐标  

AI 必须避免混用。

---

## 8.3 建议的玩家/队伍结构

```
Player_1
    └── team1
```

## 8.4 单位属性建议默认填充

```csharp
unit.Enable = true;
unit.Targetable = true;
unit.Indestructible = false;
```

---

# 9. 完整示例

```csharp
var ra3map = Ra3MapFacade.NewMap(200, 300, 10, 2, "Grass_Geneva01");

ra3map.SetTerrainHeight(30, 40, 200);
ra3map.UpdatePassabilityMap();
ra3map.SetTileTexture(30, 40, "Dirt_Yucatan03");

var p1 = ra3map.AddPlayer("Player_1");
p1.Color = "Blue";

var team1 = ra3map.AddTeam("team1", "Player_1");

var u = ra3map.AddUnitObject("SovietSurveyor", 100, 200);
u.ObjName = "u_survey_1";
u.BelongToTeam = "Player_1/team1";

ra3map.SaveAs(Ra3PathUtil.RA3MapFolder, "auto_generated_map");
```

---

# 完
