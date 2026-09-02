Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-Ra3MapUtilsRepositoryRoot {
    return [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
}

function Assert-AssetCenterVersion {
    param([Parameter(Mandatory)][string]$Version)

    $pattern = '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$'
    if ($Version -notmatch $pattern) {
        throw "Version '$Version' is invalid. Supply an explicit SemVer value such as 2.0.0 or 2.0.0-beta.1."
    }
}

function Resolve-AssetCenterFile {
    param(
        [string]$Path,
        [Parameter(Mandatory)][string]$EnvironmentVariable,
        [string]$DefaultPath,
        [Parameter(Mandatory)][string]$Description
    )

    $candidate = $Path
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        $candidate = [Environment]::GetEnvironmentVariable($EnvironmentVariable)
    }
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        $candidate = $DefaultPath
    }
    if ([string]::IsNullOrWhiteSpace($candidate) -or !(Test-Path -LiteralPath $candidate -PathType Leaf)) {
        throw "$Description is unavailable. Pass its path or set $EnvironmentVariable."
    }

    return (Resolve-Path -LiteralPath $candidate).Path
}

function Reset-AssetCenterOutputDirectory {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$AllowedRoot
    )

    $fullPath = [IO.Path]::GetFullPath($Path)
    $fullRoot = [IO.Path]::GetFullPath($AllowedRoot).TrimEnd([IO.Path]::DirectorySeparatorChar)
    $prefix = $fullRoot + [IO.Path]::DirectorySeparatorChar
    if (!$fullPath.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to reset output outside '$fullRoot'."
    }
    if (Test-Path -LiteralPath $fullPath) {
        Remove-Item -LiteralPath $fullPath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
    return $fullPath
}

function Invoke-AssetCenterCommand {
    param(
        [Parameter(Mandatory)][string]$FilePath,
        [Parameter(Mandatory)][string[]]$Arguments,
        [Parameter(Mandatory)][string]$Description
    )

    $output = @(& $FilePath @Arguments 2>&1)
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        $message = ($output | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
        throw "$Description failed with exit code $exitCode.$([Environment]::NewLine)$message"
    }
    return ($output | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
}

function ConvertFrom-AssetCenterCommandJson {
    param(
        [Parameter(Mandatory)][string]$Json,
        [Parameter(Mandatory)][string]$Description
    )

    try {
        $document = $Json | ConvertFrom-Json
    }
    catch {
        throw "$Description returned invalid JSON."
    }
    if (!$document.ok) {
        throw "$Description did not report success."
    }
    return $document
}

function Write-AssetCenterJson {
    param([Parameter(Mandatory)]$Value)

    $Value | ConvertTo-Json -Depth 16 -Compress
}
