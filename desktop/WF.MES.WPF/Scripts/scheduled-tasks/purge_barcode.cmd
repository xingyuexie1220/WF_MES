@echo off
setlocal EnableExtensions

set "SCRIPT_DIR=%~dp0"
set "LOG_DIR=%SCRIPT_DIR%logs"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

set "ENV_FILE=%SCRIPT_DIR%purge_barcode.env.cmd"
if not exist "%ENV_FILE%" (
    echo [%date% %time%] 缺少配置文件：%ENV_FILE%
    echo 请复制 purge_barcode.env.example.cmd 为 purge_barcode.env.cmd 并修改连接信息。
    exit /b 1
)

call "%ENV_FILE%"

if not defined SQL_SERVER set SQL_SERVER=127.0.0.1
if not defined SQL_DATABASE set SQL_DATABASE=NMES
if not defined RETENTION_DAYS set RETENTION_DAYS=30
if not defined BATCH_SIZE set BATCH_SIZE=5000

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd"') do set LOG_DATE=%%i
set "LOG_FILE=%LOG_DIR%\purge_%LOG_DATE%.log"

set "SQLCMD="
for %%P in (
    "%ProgramFiles%\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE"
    "%ProgramFiles%\Microsoft SQL Server\Client SDK\ODBC\160\Tools\Binn\SQLCMD.EXE"
    "%ProgramFiles%\Microsoft SQL Server\Client SDK\ODBC\150\Tools\Binn\SQLCMD.EXE"
    "%ProgramFiles%\Microsoft SQL Server\150\Tools\Binn\SQLCMD.EXE"
    "%ProgramFiles%\Microsoft SQL Server\160\Tools\Binn\SQLCMD.EXE"
) do if exist %%P set "SQLCMD=%%~P"

if not defined SQLCMD (
    where sqlcmd >nul 2>&1
    if errorlevel 1 (
        echo [%date% %time%] 未找到 sqlcmd，请安装 SQL Server 命令行工具。>> "%LOG_FILE%"
        exit /b 1
    )
    set "SQLCMD=sqlcmd"
)

set "SQL_QUERY=EXEC dbo.sp_Barcode_PurgeExpired @RetentionDays=%RETENTION_DAYS%, @BatchSize=%BATCH_SIZE%;"

echo ===== %date% %time% 开始清理 =====>> "%LOG_FILE%"
echo Server=%SQL_SERVER% Database=%SQL_DATABASE% RetentionDays=%RETENTION_DAYS%>> "%LOG_FILE%"

if "%SQL_USE_WINDOWS_AUTH%"=="1" (
    "%SQLCMD%" -S "%SQL_SERVER%" -d "%SQL_DATABASE%" -E -C -b -Q "%SQL_QUERY%" >> "%LOG_FILE%" 2>&1
) else (
    if not defined SQL_USER (
        echo [%date% %time%] 未设置 SQL_USER。>> "%LOG_FILE%"
        exit /b 1
    )
    if not defined SQL_PASSWORD (
        echo [%date% %time%] 未设置 SQL_PASSWORD。>> "%LOG_FILE%"
        exit /b 1
    )
    "%SQLCMD%" -S "%SQL_SERVER%" -d "%SQL_DATABASE%" -U "%SQL_USER%" -P "%SQL_PASSWORD%" -C -b -Q "%SQL_QUERY%" >> "%LOG_FILE%" 2>&1
)

set EXIT_CODE=%ERRORLEVEL%
if %EXIT_CODE% equ 0 (
    echo [%date% %time%] 清理成功。>> "%LOG_FILE%"
) else (
    echo [%date% %time%] 清理失败，退出码 %EXIT_CODE%。>> "%LOG_FILE%"
    echo 详情见 dbo.Barcode_PurgeLog>> "%LOG_FILE%"
)

exit /b %EXIT_CODE%
