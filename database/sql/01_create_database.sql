/*
  WF.MES - 创建数据库
  适用: SQL Server 2019+
  执行顺序: 01 -> 02 -> 03
*/

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'MES')
BEGIN
    CREATE DATABASE [MES];
END
GO

USE [MES];
GO

PRINT N'数据库 MES 已就绪';
GO
