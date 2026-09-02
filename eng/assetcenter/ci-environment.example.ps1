# Copy these variable names into the secret/variable store of the CI system you use.
# Do not copy real values into this repository.

$env:ASSETCENTER_REPO = 'C:\src\AssetCenter'
$env:ASSETCENTER_PUBLISHER_PATH = 'C:\tools\AssetCenter.Publisher.exe'
$env:RA3MAPUTILS_ASSETCENTER_PRIVATE_KEY_PATH = 'C:\secrets\ra3maputils.private.json'
$env:ASSETCENTER_7ZZ_PATH = 'C:\tools\7za.exe'
$env:ASSETCENTER_HOME = 'C:\ci-state\assetcenter-publisher'
$env:ASSETCENTER_TOKEN = '<inject-from-ci-secret-store>'
