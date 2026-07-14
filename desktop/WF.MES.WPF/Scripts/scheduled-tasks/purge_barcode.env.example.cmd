@echo off
REM 复制本文件为 purge_barcode.env.cmd 并修改连接信息（purge_barcode.env.cmd 不会提交到 Git）

set SQL_SERVER=127.0.0.1
set SQL_DATABASE=WF_MES_DEV

REM 使用 Windows 身份验证时设为 1（推荐服务器本机计划任务）
set SQL_USE_WINDOWS_AUTH=0

REM SQL 登录（SQL_USE_WINDOWS_AUTH=0 时生效）
set SQL_USER=sa
set SQL_PASSWORD=请修改为实际密码

REM 与 dbo.sp_Barcode_PurgeExpired 参数一致
set RETENTION_DAYS=30
set BATCH_SIZE=5000
