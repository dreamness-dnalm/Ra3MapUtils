[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Version,
    [string]$ReleaseId,
    [string]$BundleDirectory,
    [string]$PublisherPath = $env:ASSETCENTER_PUBLISHER_PATH,
    [string]$PrivateKeyPath = $env:RA3MAPUTILS_ASSETCENTER_PRIVATE_KEY_PATH,
    [string]$SevenZipPath = $env:ASSETCENTER_7ZZ_PATH,
    [string]$ReleaseNotesUrl,
    [ValidateRange(5, 512)][int]$PartSizeMiB = 5,
    [string]$PublisherProfile = 'ra3maputils-production'
)

. (Join-Path $PSScriptRoot 'AssetCenter.Common.ps1')
Assert-AssetCenterVersion -Version $Version

$assetId = 'cn.dreamness.ra3maputils'
$apiEndpoint = 'https://assetcenter-control-plane.dnalm-dreamness.workers.dev'
$distributionEndpoint = 'https://public-files.dreamness.cn'
$token = [Environment]::GetEnvironmentVariable('ASSETCENTER_TOKEN')
if ([string]::IsNullOrWhiteSpace($token)) {
    throw 'ASSETCENTER_TOKEN must be supplied by the process environment or CI secret store.'
}

$repositoryRoot = Get-Ra3MapUtilsRepositoryRoot
if ([string]::IsNullOrWhiteSpace($BundleDirectory)) {
    $BundleDirectory = Join-Path $repositoryRoot ".artifacts\assetcenter\$Version\bundle"
}
$bundle = [IO.Path]::GetFullPath($BundleDirectory)
$manifestPath = Join-Path $bundle 'bundle-manifest.json'
$publicKeyPath = Join-Path $repositoryRoot 'eng\assetcenter\keys\ra3maputils.public.json'
$localPrivateKey = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'AssetCenter\Ra3MapUtils\keys\ra3maputils.private.json'
$publisher = Resolve-AssetCenterFile -Path $PublisherPath -EnvironmentVariable 'ASSETCENTER_PUBLISHER_PATH' -Description 'AssetCenter Publisher executable'
$privateKey = Resolve-AssetCenterFile -Path $PrivateKeyPath -EnvironmentVariable 'RA3MAPUTILS_ASSETCENTER_PRIVATE_KEY_PATH' -DefaultPath $localPrivateKey -Description 'Ra3MapUtils signing private key'
$publicKey = Resolve-AssetCenterFile -Path $publicKeyPath -EnvironmentVariable 'RA3MAPUTILS_ASSETCENTER_PUBLIC_KEY_PATH' -Description 'Ra3MapUtils signing public key'
$manifest = Resolve-AssetCenterFile -Path $manifestPath -EnvironmentVariable 'RA3MAPUTILS_ASSETCENTER_BUNDLE_MANIFEST' -Description 'AssetCenter bundle manifest'
$sevenZip = $null
if (![string]::IsNullOrWhiteSpace($SevenZipPath)) {
    $sevenZip = Resolve-AssetCenterFile -Path $SevenZipPath -EnvironmentVariable 'ASSETCENTER_7ZZ_PATH' -Description 'Pinned 7-Zip executable'
}

if ([string]::IsNullOrWhiteSpace($ReleaseId)) {
    $createBody = [ordered]@{ schemaVersion = '1.0'; version = $Version } | ConvertTo-Json -Compress
    try {
        $created = Invoke-RestMethod -Method Post -Uri "$apiEndpoint/api/v1/assets/$assetId/releases" -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json' -Body $createBody
    }
    catch {
        throw "AssetCenter draft creation failed: $($_.Exception.Message)"
    }
    $ReleaseId = $created.data.releaseId
}
else {
    $parsedReleaseId = [Guid]::Empty
    if (![Guid]::TryParse($ReleaseId, [ref]$parsedReleaseId)) {
        throw "ReleaseId '$ReleaseId' is not a UUID."
    }
}

$configArguments = @(
    'config',
    '--profile', $PublisherProfile,
    '--api-endpoint', $apiEndpoint,
    '--distribution-endpoint', $distributionEndpoint,
    '--output', 'json'
)
[void](Invoke-AssetCenterCommand -FilePath $publisher -Arguments $configArguments -Description 'Publisher profile configuration')

try {
    $uploadArguments = [Collections.Generic.List[string]]::new()
    $uploadArguments.AddRange([string[]]@(
        'upload',
        '--profile', $PublisherProfile,
        '--asset-id', $assetId,
        '--release-id', $ReleaseId,
        '--artifact-id', 'windows-x64',
        '--manifest', $manifest,
        '--public-key-file', $publicKey,
        '--target-os', 'windows',
        '--target-architecture', 'x64',
        '--media-type', 'application/vnd.assetcenter.application',
        '--part-size-mib', $PartSizeMiB.ToString([Globalization.CultureInfo]::InvariantCulture),
        '--output', 'json'
    ))
    if ($null -ne $sevenZip) {
        $uploadArguments.Add('--7zz')
        $uploadArguments.Add($sevenZip)
    }
    $uploadJson = Invoke-AssetCenterCommand -FilePath $publisher -Arguments $uploadArguments.ToArray() -Description "AssetCenter upload for release $ReleaseId"
    $upload = ConvertFrom-AssetCenterCommandJson -Json $uploadJson -Description 'AssetCenter upload'

    $releaseManifestPath = Join-Path $bundle 'release-manifest.json'
    $finalizeArguments = [Collections.Generic.List[string]]::new()
    $finalizeArguments.AddRange([string[]]@(
        'finalize',
        '--profile', $PublisherProfile,
        '--asset-id', $assetId,
        '--release-id', $ReleaseId,
        '--artifact-id', 'windows-x64',
        '--manifest', $manifest,
        '--private-key-file', $privateKey,
        '--target-os', 'windows',
        '--target-architecture', 'x64',
        '--media-type', 'application/vnd.assetcenter.application',
        '--minimum-updater-version', '1.0.0',
        '--release-manifest-output', $releaseManifestPath,
        '--output', 'json'
    ))
    if (![string]::IsNullOrWhiteSpace($ReleaseNotesUrl)) {
        $finalizeArguments.Add('--release-notes-url')
        $finalizeArguments.Add($ReleaseNotesUrl)
    }
    if ($null -ne $sevenZip) {
        $finalizeArguments.Add('--7zz')
        $finalizeArguments.Add($sevenZip)
    }
    $finalizeJson = Invoke-AssetCenterCommand -FilePath $publisher -Arguments $finalizeArguments.ToArray() -Description "AssetCenter finalization for release $ReleaseId"
    $finalize = ConvertFrom-AssetCenterCommandJson -Json $finalizeJson -Description 'AssetCenter finalize'
}
catch {
    throw "Release $ReleaseId remains available for an explicit retry with -ReleaseId. $($_.Exception.Message)"
}

Write-AssetCenterJson -Value ([ordered]@{
    ok = $true
    assetId = $assetId
    version = $Version
    releaseId = $ReleaseId
    artifactId = 'windows-x64'
    uploadedBlockCount = $upload.data.uploadedBlockCount
    reusedBlockCount = $upload.data.reusedBlockCount
    uploadedPartCount = $upload.data.uploadedPartCount
    manifestHash = $finalize.data.manifestHash
    manifestLocation = $finalize.data.manifestLocation
    projectionState = $finalize.data.projectionState
    releaseManifestPath = $releaseManifestPath
})
