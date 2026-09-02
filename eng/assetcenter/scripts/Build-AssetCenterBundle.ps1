# Builds companion-only AssetCenter bundle from src/UI (Ra3MapUtils.exe).
# Does not pack Lua libraries or other parallel assets.
# SDK comes from src/lib AssetCenter.*.dll by default (optional ASSETCENTER_REPO for ProjectReference).
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Version,
    [string]$PublisherPath = $env:ASSETCENTER_PUBLISHER_PATH,
    [string]$PrivateKeyPath = $env:RA3MAPUTILS_ASSETCENTER_PRIVATE_KEY_PATH,
    [string]$SevenZipPath = $env:ASSETCENTER_7ZZ_PATH,
    [ValidateSet('fast', 'balanced', 'max', 'auto')][string]$Profile = 'max',
    [string]$OutputRoot
)

. (Join-Path $PSScriptRoot 'AssetCenter.Common.ps1')
Assert-AssetCenterVersion -Version $Version

$repositoryRoot = Get-Ra3MapUtilsRepositoryRoot
$projectPath = Join-Path $repositoryRoot 'src\UI\UI.csproj'
$publicKeyPath = Join-Path $repositoryRoot 'eng\assetcenter\keys\ra3maputils.public.json'
$localPrivateKey = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'AssetCenter\Ra3MapUtils\keys\ra3maputils.private.json'
$publisher = Resolve-AssetCenterFile -Path $PublisherPath -EnvironmentVariable 'ASSETCENTER_PUBLISHER_PATH' -Description 'AssetCenter Publisher executable'
$privateKey = Resolve-AssetCenterFile -Path $PrivateKeyPath -EnvironmentVariable 'RA3MAPUTILS_ASSETCENTER_PRIVATE_KEY_PATH' -DefaultPath $localPrivateKey -Description 'Ra3MapUtils signing private key'
$publicKey = Resolve-AssetCenterFile -Path $publicKeyPath -EnvironmentVariable 'RA3MAPUTILS_ASSETCENTER_PUBLIC_KEY_PATH' -Description 'Ra3MapUtils signing public key'
$sevenZip = $null
if (![string]::IsNullOrWhiteSpace($SevenZipPath)) {
    $sevenZip = Resolve-AssetCenterFile -Path $SevenZipPath -EnvironmentVariable 'ASSETCENTER_7ZZ_PATH' -Description 'Pinned 7-Zip executable'
}

if (!(Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "The Ra3MapUtils v2 UI project is missing at '$projectPath'."
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repositoryRoot '.artifacts\assetcenter'
}
$outputRootPath = [IO.Path]::GetFullPath($OutputRoot)
New-Item -ItemType Directory -Path $outputRootPath -Force | Out-Null
$versionRoot = Reset-AssetCenterOutputDirectory -Path (Join-Path $outputRootPath $Version) -AllowedRoot $outputRootPath
$publishDirectory = Join-Path $versionRoot 'publish\win-x64'
$bundleDirectory = Join-Path $versionRoot 'bundle'
New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null

$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
$publishArguments = @(
    'publish', $projectPath,
    '--configuration', 'Release',
    '--runtime', 'win-x64',
    '--self-contained', 'true',
    '--output', $publishDirectory,
    "-p:Version=$Version",
    "-p:InformationalVersion=$Version",
    '-p:DebugSymbols=false',
    '-p:DebugType=None'
)
if (![string]::IsNullOrWhiteSpace($env:ASSETCENTER_REPO)) {
    $publishArguments += "-p:AssetCenterRepo=$($env:ASSETCENTER_REPO)"
}
[void](Invoke-AssetCenterCommand -FilePath $dotnet -Arguments $publishArguments -Description 'Ra3MapUtils publish')

$packArguments = [Collections.Generic.List[string]]::new()
$packArguments.AddRange([string[]]@(
    'pack',
    '--source', $publishDirectory,
    '--output-directory', $bundleDirectory,
    '--profile', $Profile,
    '--entry-points', 'Ra3MapUtils.exe',
    '--private-key-file', $privateKey,
    '--output', 'json'
))
if ($null -ne $sevenZip) {
    $packArguments.Add('--7zz')
    $packArguments.Add($sevenZip)
}
$packJson = Invoke-AssetCenterCommand -FilePath $publisher -Arguments $packArguments.ToArray() -Description 'AssetCenter pack'
$pack = ConvertFrom-AssetCenterCommandJson -Json $packJson -Description 'AssetCenter pack'

$manifestPath = Join-Path $bundleDirectory 'bundle-manifest.json'
$verifyArguments = [Collections.Generic.List[string]]::new()
$verifyArguments.AddRange([string[]]@(
    'verify',
    '--manifest', $manifestPath,
    '--public-key-file', $publicKey,
    '--output', 'json'
))
if ($null -ne $sevenZip) {
    $verifyArguments.Add('--7zz')
    $verifyArguments.Add($sevenZip)
}
$verifyJson = Invoke-AssetCenterCommand -FilePath $publisher -Arguments $verifyArguments.ToArray() -Description 'AssetCenter verify'
$verify = ConvertFrom-AssetCenterCommandJson -Json $verifyJson -Description 'AssetCenter verify'

Write-AssetCenterJson -Value ([ordered]@{
    ok = $true
    version = $Version
    target = 'windows-x64'
    project = $projectPath
    publishDirectory = $publishDirectory
    bundleDirectory = $bundleDirectory
    manifestPath = $manifestPath
    profile = $Profile
    keyId = $verify.data.keyId
    manifestHash = $verify.data.manifestHash
    fileCount = $verify.data.fileCount
    blockCount = $verify.data.blockCount
    compressedSize = $verify.data.compressedSize
    expandedSize = $verify.data.expandedSize
})
