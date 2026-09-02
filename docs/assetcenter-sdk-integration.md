# Ra3MapUtils 接入 AssetCenter .NET SDK

本文面向 `src/UI` 的 Ra3MapUtils v2（.NET 10、WPF、Windows x64）。应用资产 `cn.dreamness.ra3maputils` 已在生产注册；产品版本线为 **`2.0.0`**。当前嵌入客户端固定使用 **`stable`** 通道（暂无 beta 切换 UI）。Lua 库等平行资产将使用**独立 AssetId**，不在本文实现范围内。

## 已注册配置

| 项目 | 值 |
| --- | --- |
| Asset ID | `cn.dreamness.ra3maputils` |
| Channel（客户端） | `stable`（运维侧仍可维护 `beta`） |
| 产品版本 | `2.0.0` |
| 分发端点 | `https://public-files.dreamness.cn/` |
| 遥测端点 | `https://assetcenter-control-plane.dnalm-dreamness.workers.dev/` |
| Key ID | `ed25519-sha256:QnOwhaW3do0UCg3Ri-x4sHwCGtGcgLIweQtxiiOFEEY` |
| Ed25519 公钥 | `uW-ErOGm8tB5JrRopLgkr5emv_byQcuc94uCvlo2P2Y` |
| Target | `windows-x64` |
| 客户端状态目录 | `%LOCALAPPDATA%\Ra3MapUtils\updates\app\` |

公开配置模板在 `eng/assetcenter/updater-config.template.json`，公开密钥在 `eng/assetcenter/keys/ra3maputils.public.json`。程序中只能包含公钥；私钥和 Publisher token 只属于发布端。

## 构建时引用 SDK

默认使用仓库内 vendored 程序集：`src/lib/AssetCenter.Protocol.dll`、`AssetCenter.Sdk.dll`、`AssetCenter.Installation.dll`（由 `eng/assetcenter/AssetCenter.Sdk.props` 引入）。升级 SDK 时从 AssetCenter 仓库 Release 输出复制这三份文件。

可选：设置 `ASSETCENTER_REPO`（或 `-p:AssetCenterRepo=`）改为对 SDK 源码的 `ProjectReference`，便于联调。不要把本机绝对路径写进已跟踪的 props。组织私有 NuGet feed 就绪后可再改为 `PackageReference`。

## 推荐的接入顺序

更新正在运行的 exe 需要版本目录、原子激活、健康确认和回滚；开发机 `dotnet run` 可验证检查/下载/暂存，完整激活需 Bootstrapper 安装布局。

1. 用 `eng/assetcenter/scripts` 构建并发布 **`2.0.0`**，签署 **`stable`** channel publication。
2. 初始安装器部署 `AssetCenter.Bootstrapper.exe`、`AssetCenter.Updater.exe`、公开配置和一个不可变的 Ra3MapUtils 版本目录。
3. 快捷方式启动 Bootstrapper，而不是直接指向某个版本目录里的 `Ra3MapUtils.exe`。
4. 设置页提供检查更新 / 下载暂存 / 应用并退出；存在 `ASSETCENTER_HEALTH_TOKEN` 时在主窗口就绪后确认健康。

## 嵌入 SDK 的配置

v2 通过 `IAppUpdateService` / `AssetCenterAppUpdateService` 封装伴侣资产客户端（频道硬编码 `stable`，遥测默认关闭）。示意：

```csharp
using AssetCenter.Sdk;

var localAppData = Environment.GetFolderPath(
    Environment.SpecialFolder.LocalApplicationData);

using var updates = new AssetCenterClient(new AssetCenterClientOptions
{
    AssetId = "cn.dreamness.ra3maputils",
    Channel = "stable",
    DistributionEndpoint = new Uri("https://public-files.dreamness.cn/"),
    TrustedKeys =
    [
        new AssetCenterTrustedKey(
            "ed25519-sha256:QnOwhaW3do0UCg3Ri-x4sHwCGtGcgLIweQtxiiOFEEY",
            "uW-ErOGm8tB5JrRopLgkr5emv_byQcuc94uCvlo2P2Y"),
    ],
    Target = AssetCenterTarget.DetectCurrent("1.0.0"),
    UpdatePolicy = AssetCenterUpdatePolicy.Prompt,
    StateDirectory = Path.Combine(localAppData, "Ra3MapUtils", "updates", "app"),
    TelemetryPreference = AssetCenterTelemetryPreference.Disabled,
    TelemetryEndpoint = null,
});
```

`DetectCurrent("1.0.0")` 中的版本是更新器/接入层协议版本，不是 Ra3MapUtils 的应用版本。它用于匹配 release 声明的 `minimumUpdaterVersion`，更新接入层时应同步修改。

初始公钥必须随受信任的安装器或代码签名配置部署，不能从同一个未验证的下载端点动态获取。将来轮换密钥时，先发布同时信任旧/新公钥的 Ra3MapUtils，再用新私钥签署 release/channel。

## 检查、下载和展示进度

WPF 只负责交互；安全检查、断点续传、7z 解码限制和哈希校验交给 SDK：

```csharp
updates.LifecycleChanged += (_, e) =>
{
    // Dispatch 到 WPF UI 线程，按 e.Stage 更新状态；日志只记录稳定的 ErrorCode。
};

var progress = new Progress<AssetCenterProgress>(value =>
{
    var ratio = value.TotalBytes == 0
        ? 0
        : (double)value.CompletedBytes / value.TotalBytes;
    updateViewModel.Report(value.Stage, ratio);
});

var check = await updates.CheckForUpdateAsync(cancellationToken);
if (check.Status == AssetCenterUpdateStatus.Available)
{
    var downloaded = await updates.DownloadArtifactAsync(
        check, progress, cancellationToken);
    var staged = await updates.StageArtifactAsync(
        check, downloaded, progress, cancellationToken);

    // staged.Path 已验证，但不能覆盖当前正在运行的程序。
    // 把它交给 Updater/ActivationCoordinator，退出后由 Bootstrapper 激活。
    await ScheduleVerifiedVersionOnExitAsync(staged.Path, cancellationToken);
}
```

也可以使用 `RunUpdatePolicyAsync` 的 `NotifyAsync`、`PromptAsync`、`ReadyToApplyAsync` 和 `ScheduleApplyOnExitAsync` hooks。Hook 是 UI/宿主边界，抛出的异常会变成稳定错误码；它不应该在 SDK 回调里直接覆盖安装目录。

SDK 会独立验证 channel 签名、有效期和 sequence，release manifest 的签名与 hash，目标兼容性，下载块 hash，7z/LZMA2 资源上限及展开后的文件树。`NotEligible` 表示不在当前灰度群组，`NotApplicable` 表示没有匹配的 target，`Withdrawn` 表示 channel 已撤回；这些状态都不应提示成网络错误。

## 版本解析与比较

发布脚本强制 release 版本符合 SemVer。SDK 内部实现了 SemVer 比较，用于判断当前更新器是否满足 artifact 的 `minimumUpdaterVersion`，但目前没有公开的通用“解析/比较任意应用版本”API，也不靠比较字符串来决定是否更新。

客户端真正的更新顺序由已签名 channel 的单调递增 `sequence` 决定；回滚到旧应用版本同样使用更高 sequence，因此不会被误判为重放。Ra3MapUtils UI 如果需要展示或排序版本，建议引用一个独立的 SemVer 库，或者等 AssetCenter 把版本类型作为公开 API 后再统一。

## 离线升级包

当前 `AssetCenterClient` 没有“指定任意本地 7z 文件并安装”的 API。这个限制是有意的：普通 7z 缺少签名 release/channel 上下文、目标约束和可回滚激活状态。

离线升级能力应该由 AssetCenter 的 Updater/Installation 层提供，SDK 只复用同一套签名、hash、解码限制和 staging 逻辑。合格的离线介质应包含完整且已签名的 release manifest、`bundle-manifest.json` 和所有 content-addressed blocks；导入后仍需验签、检查 asset ID/target/sequence，再通过 Bootstrapper 激活。这个导入命令尚未在当前客户端实现，因此第一阶段不要在 Ra3MapUtils 自行增加“解压到程序目录”的旁路。

## 安装目录、用户数据与健康确认

推荐布局如下；具体版本 ID 由 Installation 层管理：

```text
C:\Program Files\Ra3MapUtils\
  AssetCenter.Bootstrapper.exe
  updater\
  versions\
    <immutable-version-id>\
      Ra3MapUtils.exe
      ...
  updater-config.json

%LOCALAPPDATA%\Ra3MapUtils\
  settings\
  logs\
  updates\
```

用户配置、地图工作目录、缓存和日志必须位于版本目录之外，否则切换/回滚版本时会丢失或把用户数据误打包。初始安装器和 Bootstrapper 应使用操作系统代码签名。

快捷方式通过下列方式启动：

```powershell
AssetCenter.Bootstrapper.exe `
  --install-root 'C:\Program Files\Ra3MapUtils' `
  --health-timeout-seconds 60
```

Bootstrapper 为待激活版本设置：

- `ASSETCENTER_INSTALL_ROOT`
- `ASSETCENTER_VERSION_ID`
- `ASSETCENTER_HEALTH_TOKEN`（仅待确认版本）
- `ASSETCENTER_UPDATER_PATH`（存在已验证更新器时）

Ra3MapUtils 完成配置加载、核心服务初始化和主窗口可交互后，再确认健康；不要刚进入 `Main` 就确认：

```powershell
AssetCenter.Updater.exe health-confirm `
  --install-root $env:ASSETCENTER_INSTALL_ROOT `
  --token $env:ASSETCENTER_HEALTH_TOKEN
```

如果超时、进程崩溃或 token 不匹配，下次启动回退到上一个保留版本。用户手动回滚使用 Updater 的 `rollback`；全体用户回滚则发布一个更高 sequence、指向旧 release 的签名 channel publication。

## 匿名埋点与 BI

遥测默认关闭。只有用户明确同意后才启用 `Anonymous`；更新功能不能以同意遥测为条件。SDK 自动产生 `update_check`、`update_available`、`download_started/completed`、`install_started/completed`，并允许显式记录 `healthy_start`、`rollback`。

```csharp
await updates.RecordTelemetryEventAsync(new AssetCenterTelemetryEvent(
    EventId: Guid.NewGuid(),
    Name: AssetCenterTelemetryEventName.HealthyStart,
    OccurredAt: DateTimeOffset.UtcNow,
    Result: AssetCenterTelemetryResult.Success,
    Channel: userSettings.ReceiveBetaUpdates ? "beta" : "stable",
    Version: currentApplicationVersion,
    Target: "windows-x64"));

await updates.FlushTelemetryAsync();
```

不要上传用户名、文件路径、地图名、任意异常文本、IP 或业务行为。只使用协议定义的事件、稳定错误类别和匿名安装 ID。队列有事件数/字节数/时长上限，发送失败不会中断更新。

数据进入 Cloudflare Analytics Engine，聚合结果写入 D1；拥有 Ra3MapUtils `bi_viewer` 角色的操作者可以从管理端/BI API 查看按 channel、版本、target 和结果聚合的趋势。BI 是运维统计，不替代 release/channel 审计记录。健康启动和回滚需由激活层主动上报；未实现这两个事件时，BI 的安装成功率和回滚率会有缺口。

## 首次联调验收

1. 使用未发布过的 `2.0.0-beta.N` 构建并本地 `verify`。
2. 最终化 release 后，先签署 `beta` 低比例 publication。
3. 测试机从 Bootstrapper 启动，完成检查、断点续传、staging、退出激活和健康确认。
4. 验证断网仍可启动当前版本；篡改 block/manifest 会在激活前失败。
5. 模拟新版本启动失败并确认自动回退；再验证手动 rollback。
6. 用户拒绝遥测时无请求；同意后 BI 能看到匿名聚合事件。
7. `beta` 达到 100% 并观察稳定后，另行签署 `stable` publication。

发布命令、Secret 变量和重试方式见 `eng/assetcenter/README.md`。
