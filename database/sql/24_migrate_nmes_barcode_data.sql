/*
  数据迁移：NMES 条码业务数据 → MES
  =====================================
  前置条件：
    1. MES 库已存在且已执行 02_create_tables.sql（或 00_rebuild_all.sql）
    2. NMES 为旧桌面客户端库（含 Barcode_* 表）
    3. 执行前请备份：
         BACKUP DATABASE MES  TO DISK = N'D:\Backup\MES_before_nmes_migrate.bak'  WITH INIT, COMPRESSION;
         BACKUP DATABASE NMES TO DISK = N'D:\Backup\NMES_before_nmes_migrate.bak' WITH INIT, COMPRESSION;

  说明：
    - 仅迁移条码业务表（7 张），不迁移 NMES 的 System_User / System_Menu 等 legacy 权限表
    - 用户/角色/菜单统一使用 MES 的 System_* 表，在 Web 后台维护
    - 若 MES 条码表已有数据，本脚本中止（避免重复迁移）

  用法 (sqlcmd)：
    sqlcmd -S 192.168.254.10\BARTENDER -U sa -P "xxx" -i 24_migrate_nmes_barcode_data.sql
*/
USE [MES];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @SourceDb SYSNAME = N'NMES';

IF DB_ID(@SourceDb) IS NULL
BEGIN
    RAISERROR(N'源库 [%s] 不存在，请确认 NMES 库名或修改脚本顶部 @SourceDb。', 16, 1, @SourceDb);
    RETURN;
END

IF OBJECT_ID(N'dbo.Barcode_Customer', N'U') IS NULL
BEGIN
    RAISERROR(N'MES 中尚无 Barcode_Customer 表，请先执行 02_create_tables.sql', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM dbo.Barcode_Customer)
BEGIN
    RAISERROR(N'MES 条码表已有数据，为安全起见中止迁移。若需强制重迁，请先清空 Barcode_* 表。', 16, 1);
    RETURN;
END

DECLARE @sql NVARCHAR(MAX);
SET @sql = N'
IF NOT EXISTS (SELECT 1 FROM ' + QUOTENAME(@SourceDb) + N'.sys.tables WHERE name = N''Barcode_Customer'')
    RAISERROR(N''源库无 Barcode_Customer 表'', 16, 1);';
EXEC sp_executesql @sql;
GO

BEGIN TRANSACTION;
BEGIN TRY

    PRINT N'[1/7] Barcode_Customer';
    SET IDENTITY_INSERT dbo.Barcode_Customer ON;
    INSERT INTO dbo.Barcode_Customer (Customer_Id, Customer_Name, Enable, CreatedBy, CreateDate, UpdatedBy, UpdatedAt)
    SELECT Customer_Id, Customer_Name, Enable, CreatedBy, CreateDate, UpdatedBy, UpdatedAt
    FROM NMES.dbo.Barcode_Customer;
    SET IDENTITY_INSERT dbo.Barcode_Customer OFF;

    PRINT N'[2/7] Barcode_MaterialRule';
    SET IDENTITY_INSERT dbo.Barcode_MaterialRule ON;
    INSERT INTO dbo.Barcode_MaterialRule (
        Rule_Id, Customer_Id, Material_No, Barcode_Length, Qa_Status,
        Attachment_Uploaded_By, Attachment_Uploaded_At, Qa_Reviewed_By, Qa_Reviewed_At, Qa_Review_Remark,
        Drawing_Image, Print_Sample_Image, CreatedBy, CreateDate, UpdatedBy, UpdatedAt)
    SELECT
        Rule_Id, Customer_Id, Material_No,
        ISNULL(Barcode_Length, 0),
        ISNULL(Qa_Status, 0),
        Attachment_Uploaded_By, Attachment_Uploaded_At, Qa_Reviewed_By, Qa_Reviewed_At, Qa_Review_Remark,
        Drawing_Image, Print_Sample_Image, CreatedBy, CreateDate, UpdatedBy, UpdatedAt
    FROM NMES.dbo.Barcode_MaterialRule;
    SET IDENTITY_INSERT dbo.Barcode_MaterialRule OFF;

    PRINT N'[3/7] Barcode_RuleSegment';
    SET IDENTITY_INSERT dbo.Barcode_RuleSegment ON;
    INSERT INTO dbo.Barcode_RuleSegment (Segment_Id, Rule_Id, Sort_Order, Segment_Type, Config_Json, Include_In_ResetKey)
    SELECT Segment_Id, Rule_Id, Sort_Order, Segment_Type, Config_Json, Include_In_ResetKey
    FROM NMES.dbo.Barcode_RuleSegment;
    SET IDENTITY_INSERT dbo.Barcode_RuleSegment OFF;

    PRINT N'[4/7] Barcode_SerialCounter';
    SET IDENTITY_INSERT dbo.Barcode_SerialCounter ON;
    INSERT INTO dbo.Barcode_SerialCounter (Id, Rule_Id, Reset_Key, Current_Value, UpdatedAt)
    SELECT Id, Rule_Id, Reset_Key, Current_Value, UpdatedAt
    FROM NMES.dbo.Barcode_SerialCounter;
    SET IDENTITY_INSERT dbo.Barcode_SerialCounter OFF;

    PRINT N'[5/7] Barcode_GenerateRecord';
    SET IDENTITY_INSERT dbo.Barcode_GenerateRecord ON;
    INSERT INTO dbo.Barcode_GenerateRecord (
        Generate_Record_Id, Generate_No, Rule_Id, Material_No, Reset_Key, Print_Date,
        Quantity, Serial_Start, Serial_End, Print_Status, Last_Reprinted_At, Last_Reprinted_By, CreatedBy, CreatedAt)
    SELECT
        Generate_Record_Id, Generate_No, Rule_Id, Material_No, Reset_Key, Print_Date,
        Quantity, Serial_Start, Serial_End, Print_Status, Last_Reprinted_At, Last_Reprinted_By, CreatedBy, CreatedAt
    FROM NMES.dbo.Barcode_GenerateRecord;
    SET IDENTITY_INSERT dbo.Barcode_GenerateRecord OFF;

    PRINT N'[6/7] Barcode_Record';
    SET IDENTITY_INSERT dbo.Barcode_Record ON;
    INSERT INTO dbo.Barcode_Record (Record_Id, Generate_Record_Id, Rule_Id, Barcode, Reset_Key, Serial_Value, Status, CreatedAt)
    SELECT Record_Id, Generate_Record_Id, Rule_Id, Barcode, Reset_Key, Serial_Value, Status, CreatedAt
    FROM NMES.dbo.Barcode_Record;
    SET IDENTITY_INSERT dbo.Barcode_Record OFF;

    IF OBJECT_ID(N'NMES.dbo.Barcode_PurgeLog', N'U') IS NOT NULL
    BEGIN
        PRINT N'[7/7] Barcode_PurgeLog';
        SET IDENTITY_INSERT dbo.Barcode_PurgeLog ON;
        INSERT INTO dbo.Barcode_PurgeLog (PurgeLog_Id, RunAt, CutoffDate, DeletedRecordCount, DeletedGenerateCount, DurationMs, Status, Message)
        SELECT PurgeLog_Id, RunAt, CutoffDate, DeletedRecordCount, DeletedGenerateCount, DurationMs, Status, Message
        FROM NMES.dbo.Barcode_PurgeLog;
        SET IDENTITY_INSERT dbo.Barcode_PurgeLog OFF;
    END
    ELSE
        PRINT N'[7/7] Barcode_PurgeLog 源表不存在，跳过';

    /* 重置 IDENTITY 种子 */
    DECLARE @maxCustomer INT = ISNULL((SELECT MAX(Customer_Id) FROM dbo.Barcode_Customer), 0);
    DECLARE @maxRule INT = ISNULL((SELECT MAX(Rule_Id) FROM dbo.Barcode_MaterialRule), 0);
    DECLARE @maxSegment INT = ISNULL((SELECT MAX(Segment_Id) FROM dbo.Barcode_RuleSegment), 0);
    DECLARE @maxCounter INT = ISNULL((SELECT MAX(Id) FROM dbo.Barcode_SerialCounter), 0);
    DECLARE @maxGenerate INT = ISNULL((SELECT MAX(Generate_Record_Id) FROM dbo.Barcode_GenerateRecord), 0);
    DECLARE @maxRecord INT = ISNULL((SELECT MAX(Record_Id) FROM dbo.Barcode_Record), 0);
    DECLARE @maxPurge INT = ISNULL((SELECT MAX(PurgeLog_Id) FROM dbo.Barcode_PurgeLog), 0);

    IF @maxCustomer > 0 DBCC CHECKIDENT ('dbo.Barcode_Customer', RESEED, @maxCustomer);
    IF @maxRule > 0 DBCC CHECKIDENT ('dbo.Barcode_MaterialRule', RESEED, @maxRule);
    IF @maxSegment > 0 DBCC CHECKIDENT ('dbo.Barcode_RuleSegment', RESEED, @maxSegment);
    IF @maxCounter > 0 DBCC CHECKIDENT ('dbo.Barcode_SerialCounter', RESEED, @maxCounter);
    IF @maxGenerate > 0 DBCC CHECKIDENT ('dbo.Barcode_GenerateRecord', RESEED, @maxGenerate);
    IF @maxRecord > 0 DBCC CHECKIDENT ('dbo.Barcode_Record', RESEED, @maxRecord);
    IF @maxPurge > 0 DBCC CHECKIDENT ('dbo.Barcode_PurgeLog', RESEED, @maxPurge);

    COMMIT TRANSACTION;

    PRINT N'';
    PRINT N'========== 迁移完成 ==========';
    SELECT N'Barcode_Customer' AS [Table], COUNT(*) AS [Rows] FROM dbo.Barcode_Customer
    UNION ALL SELECT N'Barcode_MaterialRule', COUNT(*) FROM dbo.Barcode_MaterialRule
    UNION ALL SELECT N'Barcode_RuleSegment', COUNT(*) FROM dbo.Barcode_RuleSegment
    UNION ALL SELECT N'Barcode_SerialCounter', COUNT(*) FROM dbo.Barcode_SerialCounter
    UNION ALL SELECT N'Barcode_GenerateRecord', COUNT(*) FROM dbo.Barcode_GenerateRecord
    UNION ALL SELECT N'Barcode_Record', COUNT(*) FROM dbo.Barcode_Record
    UNION ALL SELECT N'Barcode_PurgeLog', COUNT(*) FROM dbo.Barcode_PurgeLog;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO

PRINT N'后续步骤：';
PRINT N'  1. 修改 desktop appsettings.json：Database=MES';
PRINT N'  2. Web 后台为角色分配桌面端条码菜单权限';
PRINT N'  3. 验证 WPF 条码模块读写正常';
GO
