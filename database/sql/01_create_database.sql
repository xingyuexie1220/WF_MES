/*
  WF.MES - 创建数据库
  适用: SQL Server 2019+
  执行顺序: 01 -> 02 -> 03
*/

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'WF_MES_DEV')
BEGIN
    CREATE DATABASE [WF_MES_DEV];
END
GO

USE [WF_MES_DEV];
GO

PRINT N'Database WF_MES_DEV is ready';
GO
