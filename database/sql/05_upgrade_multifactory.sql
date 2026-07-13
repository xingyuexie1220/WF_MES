/*
  WF.MES - 已有 MES 库增量升级（多工厂骨架）
  适用: 旧版 System_User 无 DefaultFactoryId、无 System_Region/System_Factory 等
  执行: sqlcmd -S "192.168.254.10\BARTENDER" -U sa -P "123456" -d MES -i 05_upgrade_multifactory.sql

  若可接受清空数据，更推荐: 00_rebuild_all.sql
*/

USE [MES];
GO

SET NOCOUNT ON;
PRINT N'[05] 开始多工厂结构升级...';
GO

/* ========== System_Region ========== */
IF OBJECT_ID(N'dbo.System_Region', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.System_Region (
        Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RegionCode  NVARCHAR(64) NOT NULL,
        RegionName  NVARCHAR(128) NOT NULL,
        Sort        INT NOT NULL CONSTRAINT DF_System_Region_Sort DEFAULT (0),
        Status      INT NOT NULL CONSTRAINT DF_System_Region_Status DEFAULT (1),
        Remark      NVARCHAR(512) NULL,
        CreateTime  DATETIME NOT NULL CONSTRAINT DF_System_Region_CreateTime DEFAULT (GETDATE()),
        CreateBy    BIGINT NULL,
        UpdateTime  DATETIME NULL,
        UpdateBy    BIGINT NULL,
        IsDeleted   BIT NOT NULL CONSTRAINT DF_System_Region_IsDeleted DEFAULT (0)
    );
    CREATE UNIQUE INDEX UX_System_Region_RegionCode ON dbo.System_Region(RegionCode) WHERE IsDeleted = 0;
    PRINT N'  + 创建 System_Region';
END
GO

/* ========== System_Factory ========== */
IF OBJECT_ID(N'dbo.System_Factory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.System_Factory (
        Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RegionId    BIGINT NOT NULL,
        FactoryCode NVARCHAR(64) NOT NULL,
        FactoryName NVARCHAR(128) NOT NULL,
        TimeZone    NVARCHAR(64) NULL,
        Sort        INT NOT NULL CONSTRAINT DF_System_Factory_Sort DEFAULT (0),
        Status      INT NOT NULL CONSTRAINT DF_System_Factory_Status DEFAULT (1),
        Remark      NVARCHAR(512) NULL,
        CreateTime  DATETIME NOT NULL CONSTRAINT DF_System_Factory_CreateTime DEFAULT (GETDATE()),
        CreateBy    BIGINT NULL,
        UpdateTime  DATETIME NULL,
        UpdateBy    BIGINT NULL,
        IsDeleted   BIT NOT NULL CONSTRAINT DF_System_Factory_IsDeleted DEFAULT (0)
    );
    CREATE UNIQUE INDEX UX_System_Factory_FactoryCode ON dbo.System_Factory(FactoryCode) WHERE IsDeleted = 0;
    PRINT N'  + 创建 System_Factory';
END
GO

IF OBJECT_ID(N'dbo.System_Factory_Config', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.System_Factory_Config (
        Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FactoryId   BIGINT NOT NULL,
        ConfigKey   NVARCHAR(128) NOT NULL,
        ConfigValue NVARCHAR(MAX) NULL,
        CreateTime  DATETIME NOT NULL CONSTRAINT DF_System_Factory_Config_CreateTime DEFAULT (GETDATE()),
        UpdateTime  DATETIME NULL
    );
    CREATE UNIQUE INDEX UX_System_Factory_Config ON dbo.System_Factory_Config(FactoryId, ConfigKey);
    PRINT N'  + 创建 System_Factory_Config';
END
GO

/* ========== System_User.DefaultFactoryId ========== */
IF COL_LENGTH(N'dbo.[System_User]', N'DefaultFactoryId') IS NULL
BEGIN
    ALTER TABLE dbo.[System_User] ADD DefaultFactoryId BIGINT NULL;
    PRINT N'  + System_User.DefaultFactoryId';
END
GO

/* ========== System_Dept.FactoryId ========== */
IF OBJECT_ID(N'dbo.System_Dept', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.System_Dept', N'FactoryId') IS NULL
BEGIN
    ALTER TABLE dbo.System_Dept ADD FactoryId BIGINT NULL;
    PRINT N'  + System_Dept.FactoryId';
END
GO

/* ========== System_User_Factory ========== */
IF OBJECT_ID(N'dbo.System_User_Factory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.System_User_Factory (
        Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId      BIGINT NOT NULL,
        FactoryId   BIGINT NOT NULL,
        IsDefault   BIT NOT NULL CONSTRAINT DF_System_User_Factory_IsDefault DEFAULT (0)
    );
    CREATE UNIQUE INDEX UX_System_User_Factory ON dbo.System_User_Factory(UserId, FactoryId);
    PRINT N'  + 创建 System_User_Factory';
END
GO

/* ========== System_Refresh_Token 扩展 ========== */
IF OBJECT_ID(N'dbo.System_Refresh_Token', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.System_Refresh_Token', N'FactoryId') IS NULL
    BEGIN
        ALTER TABLE dbo.System_Refresh_Token ADD FactoryId BIGINT NULL;
        PRINT N'  + System_Refresh_Token.FactoryId';
    END
    IF COL_LENGTH(N'dbo.System_Refresh_Token', N'SessionId') IS NULL
    BEGIN
        ALTER TABLE dbo.System_Refresh_Token ADD SessionId NVARCHAR(64) NULL;
        PRINT N'  + System_Refresh_Token.SessionId';
    END
END
GO

/* ========== System_Menu.I18nKey ========== */
IF OBJECT_ID(N'dbo.System_Menu', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.System_Menu', N'I18nKey') IS NULL
BEGIN
    ALTER TABLE dbo.System_Menu ADD I18nKey NVARCHAR(128) NULL;
    PRINT N'  + System_Menu.I18nKey';
END
GO

/* ========== 种子：默认地区/工厂（若为空） ========== */
IF NOT EXISTS (SELECT 1 FROM dbo.System_Region WHERE IsDeleted = 0)
BEGIN
    SET IDENTITY_INSERT dbo.System_Region ON;
    INSERT INTO dbo.System_Region (Id, RegionCode, RegionName, Sort, Status, CreateTime, IsDeleted)
    VALUES (1, N'SOUTH-CN', N'华南地区', 1, 1, GETDATE(), 0);
    SET IDENTITY_INSERT dbo.System_Region OFF;
    PRINT N'  + 种子 System_Region';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.System_Factory WHERE IsDeleted = 0)
BEGIN
    SET IDENTITY_INSERT dbo.System_Factory ON;
    INSERT INTO dbo.System_Factory (Id, RegionId, FactoryCode, FactoryName, TimeZone, Sort, Status, CreateTime, IsDeleted)
    VALUES (1, 1, N'FAB-DG-A', N'WF东莞A厂', N'Asia/Shanghai', 1, 1, GETDATE(), 0);
    SET IDENTITY_INSERT dbo.System_Factory OFF;
    PRINT N'  + 种子 System_Factory (FAB-DG-A)';
END
GO

/* 已有部门补 FactoryId=1 */
IF OBJECT_ID(N'dbo.System_Dept', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.System_Dept', N'FactoryId') IS NOT NULL
BEGIN
    UPDATE dbo.System_Dept SET FactoryId = 1 WHERE FactoryId IS NULL AND IsDeleted = 0;
END
GO

/* 已有用户：默认工厂 + 用户工厂关联 */
UPDATE u
SET u.DefaultFactoryId = 1
FROM dbo.[System_User] u
WHERE u.IsDeleted = 0 AND u.DefaultFactoryId IS NULL;
GO

INSERT INTO dbo.System_User_Factory (UserId, FactoryId, IsDefault)
SELECT u.Id, 1, 1
FROM dbo.[System_User] u
WHERE u.IsDeleted = 0
  AND NOT EXISTS (
      SELECT 1 FROM dbo.System_User_Factory uf WHERE uf.UserId = u.Id AND uf.FactoryId = 1
  );
GO

PRINT N'[05] 多工厂结构升级完成。请重启 API 后重试登录。';
GO
