$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$sourceDir = Join-Path $repoRoot 'i18n\messages'
$targetDir = Join-Path $repoRoot 'desktop\WF.MES.WPF\i18n'

if (-not (Test-Path $sourceDir)) {
    throw "Source directory not found: $sourceDir"
}

New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
Copy-Item -Path (Join-Path $sourceDir '*.json') -Destination $targetDir -Force
Write-Host "Synced i18n JSON to $targetDir"
Write-Host "Note: WF.MES.WPF.csproj also copies from i18n/messages at build time."
