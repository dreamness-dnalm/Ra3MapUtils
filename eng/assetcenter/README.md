# Ra3MapUtils AssetCenter 发布

这里保存 **地编伴侣应用**（`cn.dreamness.ra3maputils`）的公开注册信息、公开验签密钥和与 CI 提供商无关的 PowerShell 发布脚本。脚本只从 `src/UI/UI.csproj` 构建 v2 的 .NET 10 / Windows x64 应用，**不包含**独立版本的 Lua 库或其他平行内容资产。不读取 Git tag，不依赖 GitHub Actions 或 GitHub Releases。

首个产品版本线为 **`2.0.0`**；客户端当前固定消费 **`stable`** 通道（`beta` 仍可用于运维预发，但本仓库 v2 UI 暂无频道切换）。

## 安全边界

- 可以提交：`asset.json`、`registration.json`、`updater-config.template.json`、`keys/ra3maputils.public.json`、`AssetCenter.Sdk.props` 和脚本。
- 禁止提交：Ed25519 私钥、`ASSETCENTER_TOKEN`、Publisher `credentials.json`、R2/Cloudflare 凭据、构建产物，以及本机绝对路径形式的 SDK 引用。
- 当前工作站私钥默认位于 `%LOCALAPPDATA%\AssetCenter\Ra3MapUtils\keys\ra3maputils.private.json`。CI 应把同一密钥放进受保护的文件型 Secret，并通过 `RA3MAPUTILS_ASSETCENTER_PRIVATE_KEY_PATH` 指向它。
- 当前工作站的 Publisher 身份保存在用户本地配置中；它仅有资产级 `publisher`/`bi_viewer` 权限，没有组织级角色。CI 使用 `ASSETCENTER_TOKEN` 注入凭据，不把凭据文件复制进仓库。

仓库的 `.gitignore` 会排除 `.artifacts/assetcenter`、`eng/assetcenter/private` 和常见私钥/凭据文件名。忽略规则不是密钥保管措施，真实私钥仍必须留在仓库之外。

## 工具变量

根据 [ci-environment.example.ps1](ci-environment.example.ps1) 在本机或任意 CI 的 Secret/Variable Store 中配置：

| 变量 | 内容 |
| --- | --- |
| `ASSETCENTER_REPO` | 可选。指向 AssetCenter 源码根时改用 ProjectReference；未设置则使用仓库内 `src/lib/AssetCenter.*.dll` |
| `ASSETCENTER_PUBLISHER_PATH` | `AssetCenter.Publisher.exe` 的绝对路径 |
| `RA3MAPUTILS_ASSETCENTER_PRIVATE_KEY_PATH` | 仓库外的 Ra3MapUtils Ed25519 私钥文件 |
| `ASSETCENTER_7ZZ_PATH` | 可选的锁定版 7-Zip 可执行文件；不设时由 Publisher 解析其锁定工具链 |
| `ASSETCENTER_HOME` | 可选的 Publisher 独立状态目录，CI 中建议设置 |
| `ASSETCENTER_TOKEN` | 仅发布阶段需要的资产级 Bearer credential |

`AssetCenter.Publisher` 与私钥应由 CI 的受保护依赖/Secret 阶段落盘，任务结束后销毁临时目录。不要把 token 作为命令行参数，因为进程参数可能被其他进程读取。

SDK 默认通过 `src/lib` 中的 vendored DLL（`AssetCenter.Protocol` / `Sdk` / `Installation`）引用，见 [AssetCenter.Sdk.props](AssetCenter.Sdk.props)。升级时从 AssetCenter 仓库 Release 构建复制这三份程序集。仅在本地联调 SDK 源码时才需要设置 `ASSETCENTER_REPO`。

## 规范发布顺序（2.0.0 / stable）

```text
Build-AssetCenterBundle.ps1 -Version 2.0.0
        → Publish-AssetCenterRelease.ps1 -Version 2.0.0
        → （人工审签）stable channel publication
        → Submit-AssetCenterChannelPublication.ps1 -Channel stable ...
```

`beta` 仍可用于内部预发；面向当前 v2 客户端请最终签署并提交 **`stable`**。

## 1. 构建并验证合格资产

版本号必须显式传入且符合 SemVer。首个正式版本示例：

```powershell
$env:ASSETCENTER_REPO = '<AssetCenter 仓库根>'
$env:ASSETCENTER_PUBLISHER_PATH = '<Publisher.exe>'

& .\eng\assetcenter\scripts\Build-AssetCenterBundle.ps1 `
  -Version '2.0.0' `
  -Profile max
```

脚本执行以下不可省略的步骤：

1. `dotnet publish src/UI/UI.csproj -c Release -r win-x64 --self-contained true`（需能解析 AssetCenter SDK）；
2. 把版本写入程序集构建属性；
3. 用 Publisher 的 `max` 配置把输出切块，适合的数据块以受限 7z/LZMA2 编码；
4. 生成并签名 `bundle-manifest.json`；
5. 用仓库内的公开密钥独立执行 `verify`。

输出位于 `.artifacts/assetcenter/<version>/`，包括 `publish/win-x64` 和 `bundle`。只有脚本成功退出、最后一行 JSON 的 `ok` 为 `true`，这个目录才是合格的可上传资产。`max` 最大化压缩率；对已压缩或压缩收益不足的文件，Publisher 会保留 raw block，避免徒增解码成本。

**Companion-only：** pack 源目录仅为上述 `publish\win-x64`，入口 `Ra3MapUtils.exe`。未来平行资产（如 Lua 库）应使用**独立** `assetId` / `asset.json` / 状态目录；扩展点是“第二套 identity + 参数化源目录”，而不是把内容打进本应用 bundle。

## 2. 上传并最终化不可变 release

```powershell
$env:ASSETCENTER_TOKEN = '<由 Secret Store 注入>'

& .\eng\assetcenter\scripts\Publish-AssetCenterRelease.ps1 `
  -Version '2.0.0-beta.1'
```

脚本创建 draft、校验本地 bundle、对 R2 缺失块执行可恢复分片上传，并签名/最终化 `windows-x64` release。大文件直接上传到短期 R2 URL，不经过 Worker。块按内容寻址，相同块会跨版本复用。

如果上传或最终化中断，错误会给出 `ReleaseId`。用同一版本和该 ID 重试，不能为同一个版本盲目新建 draft：

```powershell
& .\eng\assetcenter\scripts\Publish-AssetCenterRelease.ps1 `
  -Version '2.0.0-beta.1' `
  -ReleaseId '<已有 release UUID>'
```

最终化后 release 不可覆盖。修复内容必须使用新版本号。

## 3. 发布 channel

Release 最终化不等于用户可见。`beta`/`stable` 各自维护单调递增 sequence；推广、扩大灰度、回滚和撤回都必须是新的、更高 sequence 的签名 publication。

当前仓库脚本只负责审核边界后的提交，不在 CI 中自动构造或签署 channel 文档：

```powershell
& .\eng\assetcenter\scripts\Submit-AssetCenterChannelPublication.ps1 `
  -Channel beta `
  -SignedDocumentPath 'C:\reviewed\ra3maputils-beta-publication.json'
```

签名文档必须由受控的 AssetCenter 发布者流程生成并复核 asset、channel、release、manifest hash/location、sequence、灰度比例、有效期、actor 和 key ID。脚本先做本地 envelope/身份检查，再向生产 API 提交。不要手工改动已签名 JSON；任何字节级语义变化都要求重新规范化并签名。

推荐发布顺序：`beta` 小比例 → 观察下载/安装/健康启动/回滚指标 → `beta` 100% → 另行签署并发布 `stable`。回滚不能降低 sequence，而是签署一个指向旧的不可变 release 的更高 sequence publication。

## CI 阶段建议

任意 CI 均可按以下阶段编排：

1. checkout 与依赖还原；
2. 从受保护依赖获取 Publisher/锁定 7-Zip，从 Secret Store 获取私钥文件；
3. 调用 `Build-AssetCenterBundle.ps1 -Version <显式版本>`；
4. 保存 bundle、JSON 摘要和测试结果为 CI 内部制品；
5. 仅在受保护发布任务中注入 `ASSETCENTER_TOKEN` 并调用 `Publish-AssetCenterRelease.ps1`；
6. 对 channel 文档做人工复核/签署，再调用提交脚本；
7. 清除 CI 临时 Secret 和 Publisher 状态目录。

版本输入应来自发布任务参数或产品自己的版本文件，而不是 GitHub tag。首个 v2 版本尚未由本次接入指定，因此本次只注册资产和准备流水线，不创建真实 release 或 channel publication。
