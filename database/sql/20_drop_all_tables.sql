/*
  WF.MES - 删除全部业务表（重建前执行）
  兼容 SQL Server 2012+；循环删表以处理外键依赖与历史遗留表名
*/

USE [WF_MES_DEV];
GO

SET NOCOUNT ON;

DECLARE @dropped INT = 1;
WHILE @dropped > 0
BEGIN
    SET @dropped = 0;

    DECLARE @name SYSNAME;
    DECLARE @sql NVARCHAR(MAX);
    DECLARE c CURSOR LOCAL FAST_FORWARD FOR
        SELECT name FROM sys.tables WHERE is_ms_shipped = 0 ORDER BY name;

    OPEN c;
    FETCH NEXT FROM c INTO @name;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        BEGIN TRY
            SET @sql = N'DROP TABLE dbo.' + QUOTENAME(@name);
            EXEC sp_executesql @sql;
            SET @dropped = @dropped + 1;
            PRINT N'Dropped ' + @name;
        END TRY
        BEGIN CATCH
            /* 外键依赖时跳过，下一轮再删 */
        END CATCH

        FETCH NEXT FROM c INTO @name;
    END

    CLOSE c;
    DEALLOCATE c;
END

PRINT N'All business tables dropped';
GO
