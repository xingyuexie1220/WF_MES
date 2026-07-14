/*
================================================================================
 条码数据保留与清理（SQL Server）
 策略：按 Barcode_GenerateRecord.CreatedAt 保留最近 @RetentionDays 天（默认 30）
 清理：Barcode_Record + Barcode_GenerateRecord
 保留：Barcode_SerialCounter、规则、客户等主数据
 执行：dbo.sp_Barcode_PurgeExpired，建议 SQL Server Agent 每日凌晨运行
================================================================================
*/

-- 1. 清理日志表
IF OBJECT_ID(N'dbo.Barcode_PurgeLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Barcode_PurgeLog
    (
        PurgeLog_Id          INT            IDENTITY(1, 1) NOT NULL,
        RunAt                DATETIME       NOT NULL CONSTRAINT DF_Barcode_PurgeLog_RunAt DEFAULT (GETDATE()),
        CutoffDate           DATETIME       NOT NULL,
        DeletedRecordCount   BIGINT         NOT NULL CONSTRAINT DF_Barcode_PurgeLog_Records DEFAULT (0),
        DeletedGenerateCount INT            NOT NULL CONSTRAINT DF_Barcode_PurgeLog_Generates DEFAULT (0),
        DurationMs           INT            NULL,
        Status               NVARCHAR(20)   NOT NULL,
        Message              NVARCHAR(500)  NULL,
        CONSTRAINT PK_Barcode_PurgeLog PRIMARY KEY CLUSTERED (PurgeLog_Id)
    );

    CREATE NONCLUSTERED INDEX IX_Barcode_PurgeLog_RunAt
        ON dbo.Barcode_PurgeLog (RunAt DESC);
END;
GO

-- 2. 生成单创建时间索引（加速按保留期筛选与清理）
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Barcode_GenerateRecord')
      AND name = N'IX_Barcode_GenerateRecord_CreatedAt'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Barcode_GenerateRecord_CreatedAt
        ON dbo.Barcode_GenerateRecord (CreatedAt);
END;
GO

-- 3. 清理存储过程
CREATE OR ALTER PROCEDURE dbo.sp_Barcode_PurgeExpired
    @RetentionDays INT = 30,
    @BatchSize     INT = 5000
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @RetentionDays < 1
    BEGIN
        RAISERROR(N'@RetentionDays 必须大于 0', 16, 1);
        RETURN;
    END;

    IF @BatchSize < 1
    BEGIN
        RAISERROR(N'@BatchSize 必须大于 0', 16, 1);
        RETURN;
    END;

    DECLARE @Cutoff DATETIME = DATEADD(DAY, -@RetentionDays, CAST(GETDATE() AS DATE));
    DECLARE @LogId INT;
    DECLARE @DeletedRecords BIGINT = 0;
    DECLARE @DeletedGenerates INT = 0;
    DECLARE @BatchDeleted INT;
    DECLARE @StartTime DATETIME2 = SYSDATETIME();

    INSERT INTO dbo.Barcode_PurgeLog (RunAt, CutoffDate, Status, Message)
    VALUES (GETDATE(), @Cutoff, N'Running', N'清理开始');

    SET @LogId = SCOPE_IDENTITY();

    BEGIN TRY
        WHILE 1 = 1
        BEGIN
            DELETE TOP (@BatchSize) r
            FROM dbo.Barcode_Record r
            INNER JOIN dbo.Barcode_GenerateRecord g
                ON g.Generate_Record_Id = r.Generate_Record_Id
            WHERE g.CreatedAt < @Cutoff;

            SET @BatchDeleted = @@ROWCOUNT;
            SET @DeletedRecords += @BatchDeleted;

            IF @BatchDeleted = 0
                BREAK;
        END;

        WHILE 1 = 1
        BEGIN
            DELETE TOP (@BatchSize) g
            FROM dbo.Barcode_GenerateRecord g
            WHERE g.CreatedAt < @Cutoff
              AND NOT EXISTS (
                  SELECT 1
                  FROM dbo.Barcode_Record r
                  WHERE r.Generate_Record_Id = g.Generate_Record_Id
              );

            SET @BatchDeleted = @@ROWCOUNT;
            SET @DeletedGenerates += @BatchDeleted;

            IF @BatchDeleted = 0
                BREAK;
        END;

        UPDATE dbo.Barcode_PurgeLog
        SET DeletedRecordCount = @DeletedRecords,
            DeletedGenerateCount = @DeletedGenerates,
            DurationMs = DATEDIFF(MILLISECOND, @StartTime, SYSDATETIME()),
            Status = N'Success',
            Message = CONCAT(
                N'清理完成：条码明细 ',
                @DeletedRecords,
                N' 条，生成单 ',
                @DeletedGenerates,
                N' 单；截止 ',
                CONVERT(NVARCHAR(19), @Cutoff, 120))
        WHERE PurgeLog_Id = @LogId;
    END TRY
    BEGIN CATCH
        UPDATE dbo.Barcode_PurgeLog
        SET DeletedRecordCount = @DeletedRecords,
            DeletedGenerateCount = @DeletedGenerates,
            DurationMs = DATEDIFF(MILLISECOND, @StartTime, SYSDATETIME()),
            Status = N'Failed',
            Message = LEFT(ERROR_MESSAGE(), 500)
        WHERE PurgeLog_Id = @LogId;

        THROW;
    END CATCH;
END;
GO

PRINT N'条码保留清理对象已就绪。手动试跑：EXEC dbo.sp_Barcode_PurgeExpired @RetentionDays = 30, @BatchSize = 5000;';
GO

/*
================================================================================
 4. Windows 计划任务 + sqlcmd（Express 或无 Agent 时推荐）

 步骤：
   1) 在目标业务库执行本脚本（创建 PurgeLog 表与 sp_Barcode_PurgeExpired）
   2) 复制 Scripts\scheduled-tasks\purge_barcode.env.example.cmd
      为 purge_barcode.env.cmd，填写 SQL 连接与密码
   3) 双击或命令行试跑：Scripts\scheduled-tasks\purge_barcode.cmd
   4) 管理员 PowerShell 注册每日任务：
      cd Scripts\scheduled-tasks
      .\register_purge_scheduled_task.ps1
      .\register_purge_scheduled_task.ps1 -DailyAt "02:00"

 运行日志：Scripts\scheduled-tasks\logs\purge_yyyyMMdd.log
 数据库日志：SELECT TOP 20 * FROM dbo.Barcode_PurgeLog ORDER BY RunAt DESC;

================================================================================
 5. SQL Server Agent 作业（Standard/Developer 等含 Agent 的版本可选）

 USE msdb;
 GO
 ...
================================================================================
*/
