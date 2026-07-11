/*
  WF.MES - 删除全部业务表（重建前执行）
*/

USE [MES];
GO

SET NOCOUNT ON;

DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql = @sql + N'DROP TABLE IF EXISTS dbo.' + QUOTENAME(t.name) + N';' + CHAR(13)
FROM sys.tables t
WHERE t.schema_id = SCHEMA_ID(N'dbo')
  AND t.name IN (
        N'Wh_InboundOrder', N'Prd_PassRecord', N'Prd_WorkOrder',
        N'Mds_Station', N'Mds_WorkCenter', N'Mds_Route', N'Mds_Material',
        N'Bcd_Record', N'Bcd_SerialCounter', N'Bcd_RuleSegment', N'Bcd_GenerateRecord',
        N'Bcd_MaterialRule', N'Bcd_Customer', N'Bcd_PurgeLog',
        N'Barcode_Record', N'Barcode_SerialCounter', N'Barcode_RuleSegment',
        N'Barcode_GenerateRecord', N'Barcode_MaterialRule', N'Barcode_Customer', N'Barcode_PurgeLog',
        N'Sys_Factory_Config', N'Sys_User_Factory', N'Sys_User_Position',
        N'Sys_Role_Dept', N'Sys_Role_Menu', N'Sys_User_Role', N'Sys_Refresh_Token',
        N'Sys_Operation_Log', N'Sys_Exception_Log', N'Sys_Dict_Data', N'Sys_Notice', N'Sys_Dict_Type',
        N'Sys_User', N'Sys_Position', N'Sys_Menu', N'Sys_Role', N'Sys_Dept', N'Sys_Factory', N'Sys_Region'
    );

EXEC sp_executesql @sql;
PRINT N'全部业务表已删除';
GO
