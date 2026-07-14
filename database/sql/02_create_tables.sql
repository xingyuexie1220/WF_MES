/*
  WF.MES - 创建全部表结构（与 backend Domain 实体一致）
  ClientType: 1=Web  2=Mobile  3=Desktop
  MenuType:   1=目录 2=菜单 3=按钮
  DeptType:   1=车间 2=产线 3=班组
*/

USE [WF_MES_DEV];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

/* ========== System_Region ========== */
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
GO

/* ========== System_Factory ========== */
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
    IsDeleted   BIT NOT NULL CONSTRAINT DF_System_Factory_IsDeleted DEFAULT (0),
    CONSTRAINT FK_System_Factory_Region FOREIGN KEY (RegionId) REFERENCES dbo.System_Region(Id)
);
CREATE UNIQUE INDEX UX_System_Factory_FactoryCode ON dbo.System_Factory(FactoryCode) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.System_Factory_Config (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    ConfigKey   NVARCHAR(128) NOT NULL,
    ConfigValue NVARCHAR(MAX) NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_System_Factory_Config_CreateTime DEFAULT (GETDATE()),
    UpdateTime  DATETIME NULL,
    CONSTRAINT FK_System_Factory_Config_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_System_Factory_Config ON dbo.System_Factory_Config(FactoryId, ConfigKey);
GO

/* ========== System_Dept ========== */
CREATE TABLE dbo.System_Dept (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    ParentId    BIGINT NOT NULL CONSTRAINT DF_System_Dept_ParentId DEFAULT (0),
    DeptCode    NVARCHAR(64) NOT NULL,
    DeptName    NVARCHAR(128) NOT NULL,
    DeptType    INT NOT NULL,
    Sort        INT NOT NULL CONSTRAINT DF_System_Dept_Sort DEFAULT (0),
    Status      INT NOT NULL CONSTRAINT DF_System_Dept_Status DEFAULT (1),
    Remark      NVARCHAR(512) NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_System_Dept_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_System_Dept_IsDeleted DEFAULT (0),
    CONSTRAINT FK_System_Dept_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_System_Dept_Factory_DeptCode ON dbo.System_Dept(FactoryId, DeptCode) WHERE IsDeleted = 0;
CREATE INDEX IX_System_Dept_FactoryId ON dbo.System_Dept(FactoryId) WHERE IsDeleted = 0;
GO

/* ========== System_Role ========== */
CREATE TABLE dbo.System_Role (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleCode    NVARCHAR(64) NOT NULL,
    RoleName    NVARCHAR(128) NOT NULL,
    Sort        INT NOT NULL CONSTRAINT DF_System_Role_Sort DEFAULT (0),
    DataScope   INT NOT NULL CONSTRAINT DF_System_Role_DataScope DEFAULT (3),
    Status      INT NOT NULL CONSTRAINT DF_System_Role_Status DEFAULT (1),
    Remark      NVARCHAR(512) NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_System_Role_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_System_Role_IsDeleted DEFAULT (0)
);
CREATE UNIQUE INDEX UX_System_Role_RoleCode ON dbo.System_Role(RoleCode) WHERE IsDeleted = 0;
GO

/* ========== System_Menu ========== */
CREATE TABLE dbo.System_Menu (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ParentId    BIGINT NOT NULL CONSTRAINT DF_System_Menu_ParentId DEFAULT (0),
    MenuName    NVARCHAR(128) NOT NULL,
    MenuType    INT NOT NULL,
    [Path]      NVARCHAR(256) NULL,
    Component   NVARCHAR(256) NULL,
    Permission  NVARCHAR(128) NULL,
    Icon        NVARCHAR(64) NULL,
    Sort        INT NOT NULL CONSTRAINT DF_System_Menu_Sort DEFAULT (0),
    ClientType  INT NOT NULL CONSTRAINT DF_System_Menu_ClientType DEFAULT (1),
    I18nKey     NVARCHAR(128) NULL,
    Visible     BIT NOT NULL CONSTRAINT DF_System_Menu_Visible DEFAULT (1),
    Status      INT NOT NULL CONSTRAINT DF_System_Menu_Status DEFAULT (1),
    Remark      NVARCHAR(512) NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_System_Menu_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_System_Menu_IsDeleted DEFAULT (0)
);
CREATE INDEX IX_System_Menu_ClientType ON dbo.System_Menu(ClientType) WHERE IsDeleted = 0;
GO

/* ========== System_Position ========== */
CREATE TABLE dbo.System_Position (
    Id            BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PositionCode  NVARCHAR(64) NOT NULL,
    PositionName  NVARCHAR(128) NOT NULL,
    ProcessCode   NVARCHAR(64) NULL,
    DeptId        BIGINT NULL,
    Sort          INT NOT NULL CONSTRAINT DF_System_Position_Sort DEFAULT (0),
    Status        INT NOT NULL CONSTRAINT DF_System_Position_Status DEFAULT (1),
    Remark        NVARCHAR(512) NULL,
    CreateTime    DATETIME NOT NULL CONSTRAINT DF_System_Position_CreateTime DEFAULT (GETDATE()),
    CreateBy      BIGINT NULL,
    UpdateTime    DATETIME NULL,
    UpdateBy      BIGINT NULL,
    IsDeleted     BIT NOT NULL CONSTRAINT DF_System_Position_IsDeleted DEFAULT (0)
);
CREATE UNIQUE INDEX UX_System_Position_PositionCode ON dbo.System_Position(PositionCode) WHERE IsDeleted = 0;
GO

/* ========== System_User（须加方括号，避免与 SYSTEM_USER 保留字冲突） ========== */
CREATE TABLE dbo.[System_User] (
    Id              BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserName        NVARCHAR(64) NOT NULL,
    PasswordHash    NVARCHAR(256) NOT NULL,
    NickName        NVARCHAR(64) NULL,
    Email           NVARCHAR(128) NULL,
    DeptId          BIGINT NOT NULL,
    DefaultFactoryId BIGINT NULL,
    Status          INT NOT NULL CONSTRAINT DF_System_User_Status DEFAULT (1),
    Remark          NVARCHAR(512) NULL,
    LastLoginTime   DATETIME NULL,
    CreateTime      DATETIME NOT NULL CONSTRAINT DF_System_User_CreateTime DEFAULT (GETDATE()),
    CreateBy        BIGINT NULL,
    UpdateTime      DATETIME NULL,
    UpdateBy        BIGINT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_System_User_IsDeleted DEFAULT (0),
    MustChangePassword BIT NOT NULL CONSTRAINT DF_System_User_MustChangePassword DEFAULT (0)
);
CREATE UNIQUE INDEX UX_System_User_UserName ON dbo.[System_User](UserName) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.System_User_Factory (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId      BIGINT NOT NULL,
    FactoryId   BIGINT NOT NULL,
    IsDefault   BIT NOT NULL CONSTRAINT DF_System_User_Factory_IsDefault DEFAULT (0),
    CONSTRAINT FK_System_User_Factory_User FOREIGN KEY (UserId) REFERENCES dbo.[System_User](Id),
    CONSTRAINT FK_System_User_Factory_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_System_User_Factory ON dbo.System_User_Factory(UserId, FactoryId);
GO

/* ========== 关联表 ========== */
CREATE TABLE dbo.System_User_Role (
    Id      BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId  BIGINT NOT NULL,
    RoleId  BIGINT NOT NULL
);
CREATE UNIQUE INDEX UX_System_User_Role ON dbo.System_User_Role(UserId, RoleId);
GO

CREATE TABLE dbo.System_Role_Menu (
    Id      BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleId  BIGINT NOT NULL,
    MenuId  BIGINT NOT NULL
);
CREATE UNIQUE INDEX UX_System_Role_Menu ON dbo.System_Role_Menu(RoleId, MenuId);
GO

CREATE TABLE dbo.System_Role_Dept (
    Id      BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleId  BIGINT NOT NULL,
    DeptId  BIGINT NOT NULL
);
CREATE UNIQUE INDEX UX_System_Role_Dept ON dbo.System_Role_Dept(RoleId, DeptId);
GO

CREATE TABLE dbo.System_User_Position (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId      BIGINT NOT NULL,
    PositionId  BIGINT NOT NULL
);
CREATE UNIQUE INDEX UX_System_User_Position ON dbo.System_User_Position(UserId, PositionId);
GO

/* ========== System_Refresh_Token（按端单设备会话） ========== */
CREATE TABLE dbo.System_Refresh_Token (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId      BIGINT NOT NULL,
    ClientType  INT NOT NULL CONSTRAINT DF_System_Refresh_Token_ClientType DEFAULT (1),
    FactoryId   BIGINT NULL,
    SessionId   NVARCHAR(64) NULL,
    Token       NVARCHAR(512) NOT NULL,
    ExpireTime  DATETIME NOT NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_System_Refresh_Token_CreateTime DEFAULT (GETDATE()),
    IsRevoked   BIT NOT NULL CONSTRAINT DF_System_Refresh_Token_IsRevoked DEFAULT (0)
);
CREATE INDEX IX_System_Refresh_Token_User_Client ON dbo.System_Refresh_Token(UserId, ClientType) WHERE IsRevoked = 0;
GO

/* ========== 平台配置 / 系统监控 ========== */
CREATE TABLE dbo.System_Dict_Type (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    DictName    NVARCHAR(100) NOT NULL,
    DictType    NVARCHAR(100) NOT NULL,
    Status      INT NOT NULL CONSTRAINT DF_System_Dict_Type_Status DEFAULT (1),
    Remark      NVARCHAR(500) NULL,
    CreateTime  DATETIME2 NOT NULL CONSTRAINT DF_System_Dict_Type_CreateTime DEFAULT (SYSDATETIME()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME2 NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_System_Dict_Type_IsDeleted DEFAULT (0)
);
CREATE UNIQUE INDEX UX_System_Dict_Type_DictType ON dbo.System_Dict_Type(DictType) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.System_Dict_Data (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    DictTypeId  BIGINT NOT NULL,
    DictType    NVARCHAR(100) NOT NULL,
    DictLabel   NVARCHAR(100) NOT NULL,
    DictValue   NVARCHAR(100) NOT NULL,
    Sort        INT NOT NULL CONSTRAINT DF_System_Dict_Data_Sort DEFAULT (0),
    Status      INT NOT NULL CONSTRAINT DF_System_Dict_Data_Status DEFAULT (1),
    Remark      NVARCHAR(500) NULL,
    CreateTime  DATETIME2 NOT NULL CONSTRAINT DF_System_Dict_Data_CreateTime DEFAULT (SYSDATETIME()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME2 NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_System_Dict_Data_IsDeleted DEFAULT (0)
);
CREATE INDEX IX_System_Dict_Data_DictType ON dbo.System_Dict_Data(DictType) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.System_Notice (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Title       NVARCHAR(200) NOT NULL,
    Content     NVARCHAR(MAX) NOT NULL,
    NoticeType  INT NOT NULL CONSTRAINT DF_System_Notice_NoticeType DEFAULT (1),
    Status      INT NOT NULL CONSTRAINT DF_System_Notice_Status DEFAULT (0),
    PublishTime DATETIME2 NULL,
    CreateTime  DATETIME2 NOT NULL CONSTRAINT DF_System_Notice_CreateTime DEFAULT (SYSDATETIME()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME2 NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_System_Notice_IsDeleted DEFAULT (0)
);
GO

CREATE TABLE dbo.System_Operation_Log (
    Id             BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Module         NVARCHAR(64) NULL,
    Title          NVARCHAR(128) NULL,
    BusinessType   NVARCHAR(32) NULL,
    Method         NVARCHAR(256) NULL,
    RequestMethod  NVARCHAR(16) NULL,
    OperUrl        NVARCHAR(512) NULL,
    OperIp         NVARCHAR(64) NULL,
    OperParam      NVARCHAR(MAX) NULL,
    Status         INT NOT NULL CONSTRAINT DF_System_Operation_Log_Status DEFAULT (1),
    ErrorMsg       NVARCHAR(2000) NULL,
    OperUserId     BIGINT NULL,
    OperUserName   NVARCHAR(64) NULL,
    OperTime       DATETIME2 NOT NULL CONSTRAINT DF_System_Operation_Log_OperTime DEFAULT (SYSDATETIME()),
    CostTime       BIGINT NOT NULL CONSTRAINT DF_System_Operation_Log_CostTime DEFAULT (0)
);
CREATE INDEX IX_System_Operation_Log_OperTime ON dbo.System_Operation_Log(OperTime DESC);
GO

CREATE TABLE dbo.System_Exception_Log (
    Id             BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Module         NVARCHAR(64) NULL,
    Message        NVARCHAR(2000) NOT NULL,
    StackTrace     NVARCHAR(MAX) NULL,
    RequestUrl     NVARCHAR(512) NULL,
    RequestMethod  NVARCHAR(16) NULL,
    RequestParam   NVARCHAR(MAX) NULL,
    OperIp         NVARCHAR(64) NULL,
    OperUserId     BIGINT NULL,
    OperUserName   NVARCHAR(64) NULL,
    ExceptionTime  DATETIME2 NOT NULL CONSTRAINT DF_System_Exception_Log_ExceptionTime DEFAULT (SYSDATETIME())
);
CREATE INDEX IX_System_Exception_Log_ExceptionTime ON dbo.System_Exception_Log(ExceptionTime DESC);
GO

/* ========== 条码业务表 Barcode_*（FactoryId 工厂隔离） ========== */
CREATE TABLE dbo.Barcode_Customer
(
    Customer_Id   INT            IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Factory_Id    BIGINT         NOT NULL CONSTRAINT DF_Barcode_Customer_Factory DEFAULT (1),
    Customer_Name NVARCHAR(100)  NOT NULL,
    Enable        INT            NOT NULL CONSTRAINT DF_Barcode_Customer_Enable DEFAULT (1),
    CreatedBy     NVARCHAR(50)   NULL,
    CreateDate    DATETIME       NOT NULL CONSTRAINT DF_Barcode_Customer_CreateDate DEFAULT (GETDATE()),
    UpdatedBy     NVARCHAR(50)   NULL,
    UpdatedAt     DATETIME       NULL,
    CONSTRAINT FK_Barcode_Customer_Factory FOREIGN KEY (Factory_Id) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Barcode_Customer_Factory_Name ON dbo.Barcode_Customer(Factory_Id, Customer_Name);
GO

CREATE TABLE dbo.Barcode_MaterialRule
(
    Rule_Id                INT            IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Factory_Id             BIGINT         NOT NULL CONSTRAINT DF_Barcode_MaterialRule_Factory DEFAULT (1),
    Customer_Id            INT            NOT NULL,
    Material_No            NVARCHAR(50)   NOT NULL,
    Barcode_Length         INT            NOT NULL CONSTRAINT DF_Barcode_MaterialRule_BarcodeLength DEFAULT (0),
    Qa_Status              INT            NOT NULL CONSTRAINT DF_Barcode_MaterialRule_QaStatus DEFAULT (0),
    Attachment_Uploaded_By NVARCHAR(50)   NULL,
    Attachment_Uploaded_At DATETIME       NULL,
    Qa_Reviewed_By         NVARCHAR(50)   NULL,
    Qa_Reviewed_At         DATETIME       NULL,
    Qa_Review_Remark       NVARCHAR(500)  NULL,
    Drawing_Image          VARBINARY(MAX) NULL,
    Print_Sample_Image     VARBINARY(MAX) NULL,
    CreatedBy              NVARCHAR(50)   NULL,
    CreateDate             DATETIME       NOT NULL CONSTRAINT DF_Barcode_MaterialRule_CreateDate DEFAULT (GETDATE()),
    UpdatedBy              NVARCHAR(50)   NULL,
    UpdatedAt              DATETIME       NULL,
    CONSTRAINT UQ_Barcode_MaterialRule UNIQUE (Factory_Id, Customer_Id, Material_No),
    CONSTRAINT FK_Barcode_MaterialRule_Customer FOREIGN KEY (Customer_Id) REFERENCES dbo.Barcode_Customer (Customer_Id),
    CONSTRAINT FK_Barcode_MaterialRule_Factory FOREIGN KEY (Factory_Id) REFERENCES dbo.System_Factory(Id)
);
GO

CREATE TABLE dbo.Barcode_RuleSegment
(
    Segment_Id          INT            IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Rule_Id             INT            NOT NULL,
    Sort_Order          INT            NOT NULL,
    Segment_Type        NVARCHAR(20)   NOT NULL,
    Config_Json         NVARCHAR(500)  NOT NULL,
    Include_In_ResetKey INT            NOT NULL CONSTRAINT DF_Barcode_RuleSegment_Reset DEFAULT (1),
    CONSTRAINT FK_Barcode_RuleSegment_Rule FOREIGN KEY (Rule_Id) REFERENCES dbo.Barcode_MaterialRule (Rule_Id) ON DELETE CASCADE
);
CREATE INDEX IX_Barcode_RuleSegment_Rule ON dbo.Barcode_RuleSegment (Rule_Id, Sort_Order);
GO

CREATE TABLE dbo.Barcode_SerialCounter
(
    Id            INT            IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Rule_Id       INT            NOT NULL,
    Reset_Key     NVARCHAR(500)  NOT NULL,
    Current_Value BIGINT         NOT NULL CONSTRAINT DF_Barcode_SerialCounter_Value DEFAULT (0),
    UpdatedAt     DATETIME       NOT NULL CONSTRAINT DF_Barcode_SerialCounter_Updated DEFAULT (GETDATE()),
    CONSTRAINT UQ_Barcode_SerialCounter UNIQUE (Rule_Id, Reset_Key),
    CONSTRAINT FK_Barcode_SerialCounter_Rule FOREIGN KEY (Rule_Id) REFERENCES dbo.Barcode_MaterialRule (Rule_Id)
);
GO

CREATE TABLE dbo.Barcode_GenerateRecord
(
    Generate_Record_Id INT            IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Factory_Id         BIGINT         NOT NULL CONSTRAINT DF_Barcode_GenerateRecord_Factory DEFAULT (1),
    Generate_No        NVARCHAR(50)   NOT NULL,
    Rule_Id            INT            NOT NULL,
    Material_No        NVARCHAR(50)   NOT NULL,
    Reset_Key          NVARCHAR(500)  NOT NULL,
    Print_Date         DATE           NOT NULL,
    Quantity           INT            NOT NULL,
    Serial_Start       BIGINT         NOT NULL,
    Serial_End         BIGINT         NOT NULL,
    Print_Status       INT            NOT NULL CONSTRAINT DF_Barcode_GenerateRecord_PrintStatus DEFAULT (0),
    Last_Reprinted_At  DATETIME       NULL,
    Last_Reprinted_By  NVARCHAR(50)   NULL,
    CreatedBy          NVARCHAR(50)   NULL,
    CreatedAt          DATETIME       NOT NULL CONSTRAINT DF_Barcode_GenerateRecord_Created DEFAULT (GETDATE()),
    CONSTRAINT UQ_Barcode_GenerateRecord_No UNIQUE (Generate_No),
    CONSTRAINT FK_Barcode_GenerateRecord_Rule FOREIGN KEY (Rule_Id) REFERENCES dbo.Barcode_MaterialRule (Rule_Id),
    CONSTRAINT FK_Barcode_GenerateRecord_Factory FOREIGN KEY (Factory_Id) REFERENCES dbo.System_Factory(Id)
);
CREATE INDEX IX_Barcode_GenerateRecord_CreatedAt ON dbo.Barcode_GenerateRecord (CreatedAt);
GO

CREATE TABLE dbo.Barcode_Record
(
    Record_Id          INT            IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Factory_Id         BIGINT         NOT NULL CONSTRAINT DF_Barcode_Record_Factory DEFAULT (1),
    Generate_Record_Id INT            NOT NULL,
    Rule_Id            INT            NOT NULL,
    Barcode            NVARCHAR(200)  NOT NULL,
    Reset_Key          NVARCHAR(500)  NOT NULL,
    Serial_Value       BIGINT         NOT NULL,
    Status             INT            NOT NULL CONSTRAINT DF_Barcode_Record_Status DEFAULT (1),
    CreatedAt          DATETIME       NOT NULL CONSTRAINT DF_Barcode_Record_Created DEFAULT (GETDATE()),
    CONSTRAINT UQ_Barcode_Record_Barcode UNIQUE (Barcode),
    CONSTRAINT FK_Barcode_Record_GenerateRecord FOREIGN KEY (Generate_Record_Id) REFERENCES dbo.Barcode_GenerateRecord (Generate_Record_Id),
    CONSTRAINT FK_Barcode_Record_Rule FOREIGN KEY (Rule_Id) REFERENCES dbo.Barcode_MaterialRule (Rule_Id),
    CONSTRAINT FK_Barcode_Record_Factory FOREIGN KEY (Factory_Id) REFERENCES dbo.System_Factory(Id)
);
CREATE INDEX IX_Barcode_Record_GenerateRecord ON dbo.Barcode_Record (Generate_Record_Id);
GO

CREATE TABLE dbo.Barcode_PurgeLog
(
    PurgeLog_Id          INT            IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    Factory_Id           BIGINT         NOT NULL CONSTRAINT DF_Barcode_PurgeLog_Factory DEFAULT (1),
    RunAt                DATETIME       NOT NULL CONSTRAINT DF_Barcode_PurgeLog_RunAt DEFAULT (GETDATE()),
    CutoffDate           DATETIME       NOT NULL,
    DeletedRecordCount   BIGINT         NOT NULL CONSTRAINT DF_Barcode_PurgeLog_Records DEFAULT (0),
    DeletedGenerateCount INT            NOT NULL CONSTRAINT DF_Barcode_PurgeLog_Generates DEFAULT (0),
    DurationMs           INT            NULL,
    Status               NVARCHAR(20)   NOT NULL,
    Message              NVARCHAR(500)  NULL,
    CONSTRAINT FK_Barcode_PurgeLog_Factory FOREIGN KEY (Factory_Id) REFERENCES dbo.System_Factory(Id)
);
CREATE INDEX IX_Barcode_PurgeLog_RunAt ON dbo.Barcode_PurgeLog (RunAt DESC);
GO

/* ========== 业务骨架表（Mds / Prd / Wh） ========== */
CREATE TABLE dbo.Master_Material (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    MaterialCode NVARCHAR(64) NOT NULL,
    MaterialName NVARCHAR(128) NOT NULL,
    Status      INT NOT NULL CONSTRAINT DF_Master_Material_Status DEFAULT (1),
    Remark      NVARCHAR(512) NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Master_Material_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Master_Material_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Master_Material_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Master_Material_Factory_Code ON dbo.Master_Material(FactoryId, MaterialCode) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Master_Route (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    RouteCode   NVARCHAR(64) NOT NULL,
    RouteName   NVARCHAR(128) NOT NULL,
    Status      INT NOT NULL CONSTRAINT DF_Master_Route_Status DEFAULT (1),
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Master_Route_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Master_Route_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Master_Route_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Master_Route_Factory_Code ON dbo.Master_Route(FactoryId, RouteCode) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Master_WorkCenter (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    WorkCenterCode NVARCHAR(64) NOT NULL,
    WorkCenterName NVARCHAR(128) NOT NULL,
    Status      INT NOT NULL CONSTRAINT DF_Master_WorkCenter_Status DEFAULT (1),
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Master_WorkCenter_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Master_WorkCenter_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Master_WorkCenter_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Master_WorkCenter_Factory_Code ON dbo.Master_WorkCenter(FactoryId, WorkCenterCode) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Master_Station (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    StationCode NVARCHAR(64) NOT NULL,
    StationName NVARCHAR(128) NOT NULL,
    Status      INT NOT NULL CONSTRAINT DF_Master_Station_Status DEFAULT (1),
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Master_Station_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Master_Station_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Master_Station_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Master_Station_Factory_Code ON dbo.Master_Station(FactoryId, StationCode) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Production_WorkOrder (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    WorkOrderNo NVARCHAR(64) NOT NULL,
    MaterialCode NVARCHAR(64) NULL,
    Status      INT NOT NULL CONSTRAINT DF_Production_WorkOrder_Status DEFAULT (0),
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Production_WorkOrder_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Production_WorkOrder_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Production_WorkOrder_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Production_WorkOrder_Factory_No ON dbo.Production_WorkOrder(FactoryId, WorkOrderNo) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Production_PassRecord (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    WorkOrderNo NVARCHAR(64) NOT NULL,
    StationCode NVARCHAR(64) NULL,
    Barcode     NVARCHAR(200) NULL,
    PassTime    DATETIME NOT NULL CONSTRAINT DF_Production_PassRecord_PassTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    CONSTRAINT FK_Production_PassRecord_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE INDEX IX_Production_PassRecord_Factory_Time ON dbo.Production_PassRecord(FactoryId, PassTime DESC);
GO

CREATE TABLE dbo.Warehouse_InboundOrder (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    InboundNo   NVARCHAR(64) NOT NULL,
    Status      INT NOT NULL CONSTRAINT DF_Warehouse_InboundOrder_Status DEFAULT (0),
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Warehouse_InboundOrder_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Warehouse_InboundOrder_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Warehouse_InboundOrder_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Warehouse_InboundOrder_Factory_No ON dbo.Warehouse_InboundOrder(FactoryId, InboundNo) WHERE IsDeleted = 0;
GO

/* ========== Mes_* 简易报工（CNC 外壳等） ========== */
CREATE TABLE dbo.Mes_Process (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    ProcessCode NVARCHAR(64) NOT NULL,
    ProcessName NVARCHAR(128) NOT NULL,
    DefaultSeq  INT NOT NULL CONSTRAINT DF_Mes_Process_DefaultSeq DEFAULT (0),
    Enabled     BIT NOT NULL CONSTRAINT DF_Mes_Process_Enabled DEFAULT (1),
    Remark      NVARCHAR(256) NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Mes_Process_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Mes_Process_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Mes_Process_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Mes_Process_Factory_Code ON dbo.Mes_Process(FactoryId, ProcessCode) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Mes_Routing (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    RoutingCode NVARCHAR(64) NOT NULL,
    RoutingName NVARCHAR(128) NOT NULL,
    MaterialNo  NVARCHAR(64) NULL,
    Enabled     BIT NOT NULL CONSTRAINT DF_Mes_Routing_Enabled DEFAULT (1),
    Remark      NVARCHAR(256) NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Mes_Routing_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Mes_Routing_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Mes_Routing_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Mes_Routing_Factory_Code ON dbo.Mes_Routing(FactoryId, RoutingCode) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Mes_Routing_Step (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    RoutingId   BIGINT NOT NULL,
    ProcessCode NVARCHAR(64) NOT NULL,
    Seq         INT NOT NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Mes_Routing_Step_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Mes_Routing_Step_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Mes_Routing_Step_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id),
    CONSTRAINT FK_Mes_Routing_Step_Routing FOREIGN KEY (RoutingId) REFERENCES dbo.Mes_Routing(Id)
);
CREATE INDEX IX_Mes_Routing_Step_Routing ON dbo.Mes_Routing_Step(RoutingId, Seq) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Mes_Material (
    Id           BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId    BIGINT NOT NULL,
    MaterialNo   NVARCHAR(64) NOT NULL,
    MaterialName NVARCHAR(256) NOT NULL,
    Spec         NVARCHAR(128) NULL,
    Unit         NVARCHAR(32) NULL,
    Source       NVARCHAR(32) NOT NULL CONSTRAINT DF_Mes_Material_Source DEFAULT (N'local'),
    ErpId        NVARCHAR(64) NULL,
    Enabled      BIT NOT NULL CONSTRAINT DF_Mes_Material_Enabled DEFAULT (1),
    CreateTime   DATETIME NOT NULL CONSTRAINT DF_Mes_Material_CreateTime DEFAULT (GETDATE()),
    CreateBy     BIGINT NULL,
    UpdateTime   DATETIME NULL,
    UpdateBy     BIGINT NULL,
    IsDeleted    BIT NOT NULL CONSTRAINT DF_Mes_Material_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Mes_Material_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Mes_Material_Factory_No ON dbo.Mes_Material(FactoryId, MaterialNo) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Mes_Machine (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    MachineNo   NVARCHAR(64) NOT NULL,
    MachineName NVARCHAR(128) NOT NULL,
    Enabled     BIT NOT NULL CONSTRAINT DF_Mes_Machine_Enabled DEFAULT (1),
    Remark      NVARCHAR(256) NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Mes_Machine_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Mes_Machine_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Mes_Machine_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Mes_Machine_Factory_No ON dbo.Mes_Machine(FactoryId, MachineNo) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Mes_Defect_Code (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    DefectCode  NVARCHAR(32) NOT NULL,
    DefectName  NVARCHAR(128) NOT NULL,
    Sort        INT NOT NULL CONSTRAINT DF_Mes_Defect_Code_Sort DEFAULT (0),
    Enabled     BIT NOT NULL CONSTRAINT DF_Mes_Defect_Code_Enabled DEFAULT (1),
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Mes_Defect_Code_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Mes_Defect_Code_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Mes_Defect_Code_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE UNIQUE INDEX UX_Mes_Defect_Code_Factory_Code ON dbo.Mes_Defect_Code(FactoryId, DefectCode) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Mes_Work_Order (
    Id          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId   BIGINT NOT NULL,
    WorkOrderNo NVARCHAR(64) NOT NULL,
    MaterialNo  NVARCHAR(64) NOT NULL,
    RoutingId   BIGINT NOT NULL,
    PlanQty     INT NOT NULL,
    DueDate     DATETIME NOT NULL,
    Status      NVARCHAR(16) NOT NULL CONSTRAINT DF_Mes_Work_Order_Status DEFAULT (N'open'),
    Source      NVARCHAR(32) NOT NULL CONSTRAINT DF_Mes_Work_Order_Source DEFAULT (N'local'),
    ErpBillId   NVARCHAR(64) NULL,
    SyncedAt    DATETIME NOT NULL CONSTRAINT DF_Mes_Work_Order_SyncedAt DEFAULT (GETDATE()),
    Remark      NVARCHAR(256) NULL,
    CreateTime  DATETIME NOT NULL CONSTRAINT DF_Mes_Work_Order_CreateTime DEFAULT (GETDATE()),
    CreateBy    BIGINT NULL,
    UpdateTime  DATETIME NULL,
    UpdateBy    BIGINT NULL,
    IsDeleted   BIT NOT NULL CONSTRAINT DF_Mes_Work_Order_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Mes_Work_Order_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id),
    CONSTRAINT FK_Mes_Work_Order_Routing FOREIGN KEY (RoutingId) REFERENCES dbo.Mes_Routing(Id)
);
CREATE UNIQUE INDEX UX_Mes_Work_Order_Factory_No ON dbo.Mes_Work_Order(FactoryId, WorkOrderNo) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Mes_Report_Record (
    Id              BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FactoryId       BIGINT NOT NULL,
    WorkOrderNo     NVARCHAR(64) NOT NULL,
    ProcessCode     NVARCHAR(64) NOT NULL,
    GoodQty         INT NOT NULL CONSTRAINT DF_Mes_Report_Record_GoodQty DEFAULT (0),
    DefectQty       INT NOT NULL CONSTRAINT DF_Mes_Report_Record_DefectQty DEFAULT (0),
    DefectCode      NVARCHAR(32) NULL,
    Disposition     NVARCHAR(16) NULL,
    ReworkToProcess NVARCHAR(64) NULL,
    MachineNo       NVARCHAR(64) NULL,
    OperatorName    NVARCHAR(64) NULL,
    ReportTime      DATETIME NOT NULL CONSTRAINT DF_Mes_Report_Record_ReportTime DEFAULT (GETDATE()),
    IsVoided        BIT NOT NULL CONSTRAINT DF_Mes_Report_Record_IsVoided DEFAULT (0),
    CreateTime      DATETIME NOT NULL CONSTRAINT DF_Mes_Report_Record_CreateTime DEFAULT (GETDATE()),
    CreateBy        BIGINT NULL,
    UpdateTime      DATETIME NULL,
    UpdateBy        BIGINT NULL,
    IsDeleted       BIT NOT NULL CONSTRAINT DF_Mes_Report_Record_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Mes_Report_Record_Factory FOREIGN KEY (FactoryId) REFERENCES dbo.System_Factory(Id)
);
CREATE INDEX IX_Mes_Report_Record_WO ON dbo.Mes_Report_Record(FactoryId, WorkOrderNo, ReportTime DESC) WHERE IsDeleted = 0 AND IsVoided = 0;
GO

PRINT N'Tables created';
GO
