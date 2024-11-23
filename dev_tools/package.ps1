$toolPath = Split-Path -parent $MyInvocation.MyCommand.Definition

$compileType = "Release" # Debug / Release
$projectPath = "$toolPath\..\Ra3MapUtils"
$version = Get-Content -Path "$projectPath\VERSION" -Raw
$projectFilePath = "$projectPath\Ra3MapUtils.csproj"
$buildOutPath = "$toolPath\.cache\$compileType"
$packageOutPath = "$toolPath\publish\v$version"
$releaseNotesPath = "$toolPath\..\doc\release_notes\RELEASE_v$version.md"
$softwareName = "RA3µØ±à°éÂÂ"
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

# Êä³ö
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
--releaseNotes $releaseNotesPath `
--noInst
##--icon {path}
# `

Remove-Item -Recurse -Force "$packageOutPath\RELEASES"
Remove-Item -Recurse -Force "$packageOutPath\assets.win.json"

7z x "$packageOutPath\$vpkPackId-win-Portable.zip" -o"$packageOutPath\$softwareName"
Remove-Item -Recurse -Force "$packageOutPath\$softwareName\.portable"
Remove-Item -Recurse -Force "$packageOutPath\$vpkPackId-win-Portable.zip"

7z a -t7z "$packageOutPath\$vpkPackId-v$version.7z" "$packageOutPath\$softwareName" -mx=9
#Remove-Item -Recurse -Force "$packageOutPath\$softwareName"

# Êä³ö
if ($LASTEXITCODE -eq 0) {
    Write-Host "Pack success, output path: $packageOutPath" -ForegroundColor Blue -BackgroundColor Gray
} else {
    Write-Host "Pack failed, output path: $packageOutPath" -ForegroundColor Red -BackgroundColor Gray
    throw "Pack failed with exit code: $LASTEXITCODE"
}

Invoke-Item "$packageOutPath"
    