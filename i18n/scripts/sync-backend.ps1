$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$messages = Join-Path $root 'messages\zh-CN.json'
$resourcesDir = Join-Path $root '..\backend\WF.MES.Api\Resources'
New-Item -ItemType Directory -Force -Path $resourcesDir | Out-Null

$jsonText = Get-Content $messages -Raw -Encoding UTF8
$json = $jsonText | ConvertFrom-Json

function Flatten-Json($obj, [string]$prefix = '') {
    $result = @{}
    foreach ($prop in $obj.PSObject.Properties) {
        $key = if ($prefix) { "$prefix.$($prop.Name)" } else { $prop.Name }
        if ($prop.Value -is [System.Management.Automation.PSCustomObject]) {
            foreach ($entry in (Flatten-Json $prop.Value $key).GetEnumerator()) {
                $result[$entry.Key] = $entry.Value
            }
        }
        else {
            $result[$key] = [string]$prop.Value
        }
    }
    return $result
}

function Escape-Xml([string]$value) {
    return $value.Replace('&', '&amp;').Replace('<', '&lt;').Replace('>', '&gt;').Replace('"', '&quot;')
}

$flat = Flatten-Json $json
$resxPath = Join-Path $resourcesDir 'SharedResources.zh-CN.resx'
$lines = @(
    '<?xml version="1.0" encoding="utf-8"?>',
    '<root>',
    '  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>',
    '  <resheader name="version"><value>2.0</value></resheader>',
    '  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>',
    '  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>'
)
foreach ($entry in ($flat.GetEnumerator() | Sort-Object Name)) {
    $value = Escape-Xml $entry.Value
    $lines += "  <data name=`"$($entry.Key)`" xml:space=`"preserve`"><value>$value</value></data>"
}
$lines += '</root>'
Set-Content -Path $resxPath -Value ($lines -join "`n") -Encoding UTF8
Write-Host "Synced zh-CN.json -> SharedResources.zh-CN.resx"
