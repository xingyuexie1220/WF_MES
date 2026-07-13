/*
  WF.MES - 表前缀重命名（已有库增量升级，SQL Server 2012+）
  将缩写前缀改为见名知意全词前缀：
    Bcd_  -> Barcode_
    Mds_  -> Master_
    Prd_  -> Production_
    Wh_   -> Warehouse_
    Sys_  -> System_
  用法:
    sqlcmd -S 192.168.254.10\BARTENDER -U sa -P <pwd> -d WF_MES_DEV -i 06_rename_table_prefixes.sql
  说明: 新建库请直接执行 02_create_tables.sql，无需本脚本。
*/

USE [WF_MES_DEV];
GO

SET NOCOUNT ON;

DECLARE @Renames TABLE (OldName NVARCHAR(128), NewName NVARCHAR(128));
INSERT INTO @Renames (OldName, NewName) VALUES
    (N'Bcd_PurgeLog', N'Barcode_PurgeLog'),
    (N'Bcd_Record', N'Barcode_Record'),
    (N'Bcd_SerialCounter', N'Barcode_SerialCounter'),
    (N'Bcd_RuleSegment', N'Barcode_RuleSegment'),
    (N'Bcd_GenerateRecord', N'Barcode_GenerateRecord'),
    (N'Bcd_MaterialRule', N'Barcode_MaterialRule'),
    (N'Bcd_Customer', N'Barcode_Customer'),
    (N'Wh_InboundOrder', N'Warehouse_InboundOrder'),
    (N'Prd_PassRecord', N'Production_PassRecord'),
    (N'Prd_WorkOrder', N'Production_WorkOrder'),
    (N'Mds_Station', N'Master_Station'),
    (N'Mds_WorkCenter', N'Master_WorkCenter'),
    (N'Mds_Route', N'Master_Route'),
    (N'Mds_Material', N'Master_Material'),
    (N'Sys_Exception_Log', N'System_Exception_Log'),
    (N'Sys_Operation_Log', N'System_Operation_Log'),
    (N'Sys_Notice', N'System_Notice'),
    (N'Sys_Dict_Data', N'System_Dict_Data'),
    (N'Sys_Dict_Type', N'System_Dict_Type'),
    (N'Sys_Refresh_Token', N'System_Refresh_Token'),
    (N'Sys_User_Position', N'System_User_Position'),
    (N'Sys_Role_Dept', N'System_Role_Dept'),
    (N'Sys_Role_Menu', N'System_Role_Menu'),
    (N'Sys_User_Role', N'System_User_Role'),
    (N'Sys_User_Factory', N'System_User_Factory'),
    (N'Sys_User', N'System_User'),
    (N'Sys_Position', N'System_Position'),
    (N'Sys_Menu', N'System_Menu'),
    (N'Sys_Role', N'System_Role'),
    (N'Sys_Dept', N'System_Dept'),
    (N'Sys_Factory_Config', N'System_Factory_Config'),
    (N'Sys_Factory', N'System_Factory'),
    (N'Sys_Region', N'System_Region');

DECLARE @OldName NVARCHAR(128);
DECLARE @NewName NVARCHAR(128);
DECLARE rename_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT OldName, NewName FROM @Renames ORDER BY OldName;

OPEN rename_cursor;
FETCH NEXT FROM rename_cursor INTO @OldName, @NewName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(N'dbo.' + @OldName, N'U') IS NOT NULL
       AND OBJECT_ID(N'dbo.' + @NewName, N'U') IS NULL
    BEGIN
        DECLARE @Sql NVARCHAR(300) = N'EXEC sp_rename @obj, @new, ''OBJECT'';';
        EXEC sp_executesql @Sql,
            N'@obj NVARCHAR(256), @new NVARCHAR(128)',
            @obj = N'dbo.' + @OldName,
            @new = @NewName;
        PRINT N'Renamed: ' + @OldName + N' -> ' + @NewName;
    END

    FETCH NEXT FROM rename_cursor INTO @OldName, @NewName;
END

CLOSE rename_cursor;
DEALLOCATE rename_cursor;

PRINT N'Table prefix rename completed.';
GO
