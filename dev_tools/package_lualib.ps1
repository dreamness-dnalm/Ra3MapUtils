﻿$luaLibPath = "H:\Program Files (x86)\Red Alert 3(Incomplete)\ra3_map_workspace\RA3CoronaMapLuaLib"
$excludeDirs = @(".git")

$toolPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$packageOutPath = "$toolPath\publish\lualib"
$version = Get-Content -Path "$luaLibPath\VERSION" -Raw
$output7zFileName = "Ra3CoronaMapLuaLib_v$version.7z"
$output7zPath = "$packageOutPath\$output7zFileName"
$releaseJsonFilePath = "$packageOutPath\release.json"

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
    7z a -t7z "$output7zPath" "$luaLibPath\*" -mx=9 -aoa $excludeParams

    if ($LASTEXITCODE -ne 0) {
        Write-Error "压缩失败，错误码：$LASTEXITCODE"
        exit $LASTEXITCODE
    }

    Write-Host "压缩成功完成！输出文件：$output7zPath" -ForegroundColor Green
}
finally {
    Pop-Location
}

$output7zMd5 = Get-FileHash -Path $output7zPath -Algorithm MD5

$releaseJsonFileData = @{
    "Version" = "$version"
    "Md5" = $output7zMd5.Hash
    "FileName" = $output7zFileName
}

$releaseJsonFileData | ConvertTo-Json | Out-File -FilePath $releaseJsonFilePath -Encoding utf8

Write-Host "Release json file: $releaseJsonFilePath" -ForegroundColor Green

Invoke-Item "$packageOutPath"