$toolPath = Split-Path -parent $MyInvocation.MyCommand.Definition

$compileType = "Release" # Debug / Release
$projectPath = "$toolPath\..\Ra3MapUtils"
$version = (Get-Content -Path "$projectPath\VERSION" -Raw).Trim()
$projectFilePath = "$projectPath\Ra3MapUtils.csproj"
$buildOutPath = "$toolPath\.cache\$compileType"
$packageOutPath = "$toolPath\publish\v$version"
$releaseNotesPath = "$toolPath\..\doc\release_notes\RELEASE_v$version.md"
$softwareName = "RA3�ر����"
$vpkPackId = "Ra3MapUtils"

# ----------- build ------------

if (Test-Path -Path $buildOutPath) {
    Remove-Item -Recurse -Force $buildOutPath
} 

New-item -ItemType Directory -Path $buildOutPath

dotnet publish `
    $projectFilePath `
    -c $compileType `
    -r win-x64 `
    -o $buildOutPath

# ���
if ($LASTEXITCODE -eq 0) {
    Write-Host "Build success, output path: $buildOutPath" -ForegroundColor Blue -BackgroundColor Gray
} else {
    Write-Host "Build failed, output path: $buildOutPath" -ForegroundColor Red -BackgroundColor Gray
    throw "Build failed with exit code: $LASTEXITCODE"
}

# ----------- package ------------

if(Test-Path -Path $packageOutPath) {
    Remove-Item -Recurse -Force $packageOutPath
}

New-item -ItemType Directory -Path $packageOutPath

vpk pack `
--packId $vpkPackId `
--packVersion $version `
--packDir $buildOutPath `
--packAuthors dreamness `
--packTitle $softwareName `
--exclude ".*.pdb" `
--mainExe Ra3MapUtils.exe `
--outputDir $packageOutPath `
--delta BestSize `
$(if (Test-Path $releaseNotesPath) { "--releaseNotes $releaseNotesPath" }) `
--framework net8.0-x64-sdk
# --noInst
##--icon {path}
# `

Remove-Item -Recurse -Force "$packageOutPath\RELEASES"
Remove-Item -Recurse -Force "$packageOutPath\assets.win.json"

7z x "$packageOutPath\$vpkPackId-win-Portable.zip" -o"$packageOutPath\$softwareName"
Remove-Item -Recurse -Force "$packageOutPath\$softwareName\.portable"
Remove-Item -Recurse -Force "$packageOutPath\$vpkPackId-win-Portable.zip"

# д�ı���$packageOutPath\$softwareName\�������ʧ�����Ķ����ļ�.txt 
# �ı�����Ϊ: �������: You must install or update .NET to run this application.
#�������ز���װ����https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.415/dotnet-sdk-8.0.415-win-x64.exe
# ����QQȺ��ȡ����:  513118543  /  613550502
# �ɲο����߰����ĵ�: https://www.yuque.com/muzeqaq/ra3mapwiki/vppua7qbrig4emxd

Set-Content -Path "$packageOutPath\$softwareName\�������ʧ�����Ķ����ļ�.txt" -Value "�������: You must install or update .NET to run this application.`r`n�����ز���װ����https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.415/dotnet-sdk-8.0.415-win-x64.exe`r`n����QQȺ��ȡ����:  513118543  /  613550502`r`n�ɲο����߰����ĵ�: https://www.yuque.com/muzeqaq/ra3mapwiki/vppua7qbrig4emxd" -Encoding UTF8

7z a -t7z "$packageOutPath\$vpkPackId-v$version-Portable.7z" "$packageOutPath\$softwareName" -mx=9
#Remove-Item -Recurse -Force "$packageOutPath\$softwareName"

# ���
if ($LASTEXITCODE -eq 0) {
    Write-Host "Pack success, output path: $packageOutPath" -ForegroundColor Blue -BackgroundColor Gray
} else {
    Write-Host "Pack failed, output path: $packageOutPath" -ForegroundColor Red -BackgroundColor Gray
    throw "Pack failed with exit code: $LASTEXITCODE"
}

Invoke-Item "$packageOutPath"
    