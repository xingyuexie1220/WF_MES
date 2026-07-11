#Requires -RunAsAdministrator
<#
.SYNOPSIS
    注册 Windows 计划任务：每日执行条码过期清理（调用 purge_barcode.cmd / sqlcmd）

.USAGE
    1. 复制 purge_barcode.env.example.cmd 为 purge_barcode.env.cmd 并填写连接信息
    2. 在 NMES 库已执行 Scripts\Barcode_PurgeRetention.sql
    3. 以管理员 PowerShell 运行：
       .\register_purge_scheduled_task.ps1
       .\register_purge_scheduled_task.ps1 -DailyAt "03:30"
#>
param(
    [string]$TaskName = "WF_MES_Barcode_Purge",
    [string]$DailyAt = "02:00",
    [string]$RunAsUser = "SYSTEM"
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$cmdPath = Join-Path $scriptDir "purge_barcode.cmd"
$envPath = Join-Path $scriptDir "purge_barcode.env.cmd"

if (-not (Test-Path $cmdPath)) {
    throw "未找到 $cmdPath"
}

if (-not (Test-Path $envPath)) {
    throw "未找到 $envPath，请先复制 purge_barcode.env.example.cmd 并修改连接信息。"
}

$time = [DateTime]::ParseExact($DailyAt, "H:mm", $null)

$action = New-ScheduledTaskAction -Execute $cmdPath -WorkingDirectory $scriptDir
$trigger = New-ScheduledTaskTrigger -Daily -At $time
$settings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -ExecutionTimeLimit (New-TimeSpan -Hours 2)

$existing = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($existing) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
}

Register-ScheduledTask `
    -TaskName $TaskName `
    -Action $action `
    -Trigger $trigger `
    -Settings $settings `
    -User $RunAsUser `
    -RunLevel Highest `
    -Description "WF MES：每日清理超过保留期的条码生成单与明细（sp_Barcode_PurgeExpired）" | Out-Null

Write-Host "已注册计划任务：$TaskName"
Write-Host "  执行时间：每天 $($time.ToString('HH:mm'))"
Write-Host "  运行账户：$RunAsUser"
Write-Host "  脚本路径：$cmdPath"
Write-Host ""
Write-Host "手动试跑："
Write-Host "  $cmdPath"
Write-Host ""
Write-Host "查看运行日志："
Write-Host "  $scriptDir\logs\"
Write-Host ""
Write-Host "查看数据库清理日志："
Write-Host "  SELECT TOP 20 * FROM NMES.dbo.Barcode_PurgeLog ORDER BY RunAt DESC;"
