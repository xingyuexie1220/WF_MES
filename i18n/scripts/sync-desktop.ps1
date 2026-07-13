$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$syncScript = Join-Path $repoRoot 'i18n\scripts\sync-api-codes.mjs'

if (-not (Test-Path $syncScript)) {
    throw "Script not found: $syncScript"
}

node $syncScript
Write-Host "Desktop UI pack: desktop\WF.MES.WPF\i18n\ (copied to output i18n\ at build)"
Write-Host "See i18n\README.md for architecture."
