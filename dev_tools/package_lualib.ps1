$luaLibPath = "D:\workspace\mia\ra3_map_workspace\Ra3CoronaMapLuaLib"
$excludeDirs = @(".git")

$toolPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$packageOutPath = "$toolPath\publish"
$version = Get-Content -Path "$luaLibPath\VERSION" -Raw

# 检查7-Zip是否可用
if (-not (Get-Command 7z -ErrorAction SilentlyContinue)) {
    Write-Error "7-Zip 未找到，请安装并添加到系统路径！"
    exit 1
}

# 验证源文件夹是否存在
if (-not (Test-Path $luaLibPath -PathType Container)) {
    Write-Error "源文件夹不存在：$luaLibPath"
    exit 1
}

# 构建排除参数
$excludeParams = $excludeDirs | ForEach-Object { "-xr!$_" }

# 设置压缩参数（可根据需要调整）
#$compressionLevel = "-mx=9" # 压缩级别 1-9（1最快压缩，9最佳压缩）

# 执行压缩命令
try {
    # 显示压缩信息
    Write-Host "正在压缩 $luaLibPath 到 $packageOutPath..."
    Write-Host "排除目录: $($excludeDirs -join ', ')"

    # 执行7-Zip命令
    7z a -t7z "$packageOutPath\Ra3CoronaMapLuaLib_v$version.7z" "$luaLibPath\*" -mx=9 -aoa $excludeParams

    if ($LASTEXITCODE -ne 0) {
        Write-Error "压缩失败，错误码：$LASTEXITCODE"
        exit $LASTEXITCODE
    }

    Write-Host "压缩成功完成！输出文件：$output7z" -ForegroundColor Green
}
finally {
    Pop-Location
}