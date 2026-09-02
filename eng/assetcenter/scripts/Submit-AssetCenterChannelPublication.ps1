[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('beta', 'stable')][string]$Channel,
    [Parameter(Mandatory)][string]$SignedDocumentPath
)

. (Join-Path $PSScriptRoot 'AssetCenter.Common.ps1')

$assetId = 'cn.dreamness.ra3maputils'
$keyId = 'ed25519-sha256:QnOwhaW3do0UCg3Ri-x4sHwCGtGcgLIweQtxiiOFEEY'
$apiEndpoint = 'https://assetcenter-control-plane.dnalm-dreamness.workers.dev'
$token = [Environment]::GetEnvironmentVariable('ASSETCENTER_TOKEN')
if ([string]::IsNullOrWhiteSpace($token)) {
    throw 'ASSETCENTER_TOKEN must be supplied by the process environment or CI secret store.'
}

$documentPath = Resolve-AssetCenterFile -Path $SignedDocumentPath -EnvironmentVariable 'RA3MAPUTILS_ASSETCENTER_CHANNEL_DOCUMENT' -Description 'Signed channel publication document'
$canonicalJson = Get-Content -Raw -Encoding UTF8 -LiteralPath $documentPath
try {
    $document = $canonicalJson | ConvertFrom-Json
}
catch {
    throw 'The signed channel publication is not valid JSON.'
}

$requiredProperties = @(
    'schemaVersion', 'assetId', 'channel', 'sequence', 'action', 'release',
    'rollout', 'directive', 'publishedAt', 'expiresAt', 'actor', 'signature'
)
$propertyNames = @($document.PSObject.Properties.Name)
$signaturePropertyNames = if ($propertyNames -contains 'signature' -and $null -ne $document.signature) {
    @($document.signature.PSObject.Properties.Name)
}
else {
    @()
}
$sequenceValue = if ($propertyNames -contains 'sequence') { $document.sequence } else { $null }
$sequence = 0L
$hasValidSequence = [long]::TryParse([string]$sequenceValue, [ref]$sequence) -and $sequence -ge 1
$hasRequiredProperties = $requiredProperties.Where({ $propertyNames -notcontains $_ }).Count -eq 0
$hasSignatureEnvelope = @('algorithm', 'keyId', 'value').Where({ $signaturePropertyNames -notcontains $_ }).Count -eq 0

if (!$hasRequiredProperties -or
    !$hasSignatureEnvelope -or
    $document.schemaVersion -ne '1.0' -or
    $document.assetId -ne $assetId -or
    $document.channel -ne $Channel -or
    !$hasValidSequence -or
    $document.signature.algorithm -ne 'Ed25519' -or
    $document.signature.keyId -ne $keyId -or
    [string]::IsNullOrWhiteSpace([string]$document.signature.value)) {
    throw 'The signed channel publication identity, sequence, or signature envelope is invalid.'
}

try {
    $response = Invoke-RestMethod -Method Post -Uri "$apiEndpoint/api/v1/assets/$assetId/channels/$Channel/publications" -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json' -Body $canonicalJson
}
catch {
    throw "AssetCenter $Channel publication failed: $($_.Exception.Message)"
}

Write-AssetCenterJson -Value ([ordered]@{
    ok = $true
    assetId = $assetId
    channel = $Channel
    sequence = $sequence
    action = $document.action
    version = $document.release.version
    publicationId = $response.data.publicationId
    projectionState = $response.data.state
})
