/*
  WF.MES - 业务表基础测试数据（在 03_seed_data.sql 之后执行）
  覆盖: Barcode_* / Master_* / Production_* / Warehouse_*
  工厂: Factory_Id=1 (WF东莞A厂)
  用法:
    sqlcmd -S 192.168.254.10\BARTENDER -U sa -P 123456 -d WF_MES_DEV -i 04_seed_business_data.sql
*/

USE [WF_MES_DEV];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

/* ========== 条码客户 ========== */
IF NOT EXISTS (SELECT 1 FROM dbo.Barcode_Customer WHERE Factory_Id = 1 AND Customer_Name = N'华为技术有限公司')
    INSERT INTO dbo.Barcode_Customer (Factory_Id, Customer_Name, Enable, CreatedBy)
    VALUES (1, N'华为技术有限公司', 1, N'admin');

IF NOT EXISTS (SELECT 1 FROM dbo.Barcode_Customer WHERE Factory_Id = 1 AND Customer_Name = N'富士康科技集团')
    INSERT INTO dbo.Barcode_Customer (Factory_Id, Customer_Name, Enable, CreatedBy)
    VALUES (1, N'富士康科技集团', 1, N'admin');
GO

DECLARE @CustomerHuawei INT = (SELECT Customer_Id FROM dbo.Barcode_Customer WHERE Factory_Id = 1 AND Customer_Name = N'华为技术有限公司');
DECLARE @CustomerFoxconn INT = (SELECT Customer_Id FROM dbo.Barcode_Customer WHERE Factory_Id = 1 AND Customer_Name = N'富士康科技集团');

/* ========== 物料规则 + 规则段 ========== */
/* 规则1: WF + yyyyMMdd + 4位流水，已 QA 确认，可打印。示例条码: WF202607110001 */
IF NOT EXISTS (SELECT 1 FROM dbo.Barcode_MaterialRule WHERE Factory_Id = 1 AND Material_No = N'MAT-001')
BEGIN
    INSERT INTO dbo.Barcode_MaterialRule
        (Factory_Id, Customer_Id, Material_No, Barcode_Length, Qa_Status, Qa_Reviewed_By, Qa_Reviewed_At, Qa_Review_Remark, CreatedBy)
    VALUES
        (1, @CustomerHuawei, N'MAT-001', 14, 2, N'admin', GETDATE(), N'测试数据-已确认', N'admin');

    DECLARE @Rule1 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Barcode_RuleSegment (Rule_Id, Sort_Order, Segment_Type, Config_Json, Include_In_ResetKey)
    VALUES
        (@Rule1, 1, N'Literal', N'{"value":"WF"}', 1),
        (@Rule1, 2, N'Date',    N'{"format":"yyyyMMdd"}', 1),
        (@Rule1, 3, N'Serial',   N'{"radix":10,"digits":4}', 0);
END
GO

/* 规则2: 待 QA 审核（桌面端「条码资料审核」测试用） */
DECLARE @CustomerFoxconn2 INT = (SELECT Customer_Id FROM dbo.Barcode_Customer WHERE Factory_Id = 1 AND Customer_Name = N'富士康科技集团');

IF NOT EXISTS (SELECT 1 FROM dbo.Barcode_MaterialRule WHERE Factory_Id = 1 AND Material_No = N'MAT-002')
BEGIN
    INSERT INTO dbo.Barcode_MaterialRule
        (Factory_Id, Customer_Id, Material_No, Barcode_Length, Qa_Status, CreatedBy)
    VALUES
        (1, @CustomerFoxconn2, N'MAT-002', 12, 1, N'operator');

    DECLARE @Rule2 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Barcode_RuleSegment (Rule_Id, Sort_Order, Segment_Type, Config_Json, Include_In_ResetKey)
    VALUES
        (@Rule2, 1, N'Literal', N'{"value":"FX"}', 1),
        (@Rule2, 2, N'Date',    N'{"format":"yyMMdd"}', 1),
        (@Rule2, 3, N'Serial',   N'{"radix":10,"digits":4}', 0);
END
GO

/* ========== 条码生成记录 + 明细（历史打印样例） ========== */
DECLARE @Rule1Id INT = (SELECT Rule_Id FROM dbo.Barcode_MaterialRule WHERE Factory_Id = 1 AND Material_No = N'MAT-001');

IF @Rule1Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Barcode_GenerateRecord WHERE Generate_No = N'GEN-20260711-001')
BEGIN
    INSERT INTO dbo.Barcode_GenerateRecord
        (Factory_Id, Generate_No, Rule_Id, Material_No, Reset_Key, Print_Date, Quantity, Serial_Start, Serial_End, Print_Status, CreatedBy)
    VALUES
        (1, N'GEN-20260711-001', @Rule1Id, N'MAT-001', N'WF|20260711', CAST(GETDATE() AS DATE), 3, 1, 3, 1, N'operator');

    DECLARE @GenId INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Barcode_Record (Factory_Id, Generate_Record_Id, Rule_Id, Barcode, Reset_Key, Serial_Value, Status)
    VALUES
        (1, @GenId, @Rule1Id, N'WF202607110001', N'WF|20260711', 1, 1),
        (1, @GenId, @Rule1Id, N'WF202607110002', N'WF|20260711', 2, 1),
        (1, @GenId, @Rule1Id, N'WF202607110003', N'WF|20260711', 3, 1);

    INSERT INTO dbo.Barcode_SerialCounter (Rule_Id, Reset_Key, Current_Value)
    VALUES (@Rule1Id, N'WF|20260711', 3);
END
GO

/* ========== 主数据 Master_* ========== */
IF NOT EXISTS (SELECT 1 FROM dbo.Master_Material WHERE FactoryId = 1 AND MaterialCode = N'MAT-001')
    INSERT INTO dbo.Master_Material (FactoryId, MaterialCode, MaterialName, Status, Remark, CreateBy)
    VALUES (1, N'MAT-001', N'钢板组件A', 1, N'测试物料', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Master_Material WHERE FactoryId = 1 AND MaterialCode = N'MAT-002')
    INSERT INTO dbo.Master_Material (FactoryId, MaterialCode, MaterialName, Status, Remark, CreateBy)
    VALUES (1, N'MAT-002', N'电子模组B', 1, N'测试物料', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Master_Route WHERE FactoryId = 1 AND RouteCode = N'RT-ASSY-01')
    INSERT INTO dbo.Master_Route (FactoryId, RouteCode, RouteName, Status, CreateBy)
    VALUES (1, N'RT-ASSY-01', N'组装工艺路线01', 1, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Master_WorkCenter WHERE FactoryId = 1 AND WorkCenterCode = N'WC-M01')
    INSERT INTO dbo.Master_WorkCenter (FactoryId, WorkCenterCode, WorkCenterName, Status, CreateBy)
    VALUES (1, N'WC-M01', N'机加工中心', 1, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Master_Station WHERE FactoryId = 1 AND StationCode = N'ST-SCAN-01')
    INSERT INTO dbo.Master_Station (FactoryId, StationCode, StationName, Status, CreateBy)
    VALUES (1, N'ST-SCAN-01', N'扫码工站01', 1, 1);
GO

/* ========== 生产工单 + 过站记录 ========== */
IF NOT EXISTS (SELECT 1 FROM dbo.Production_WorkOrder WHERE FactoryId = 1 AND WorkOrderNo = N'WO-20260711-001')
    INSERT INTO dbo.Production_WorkOrder (FactoryId, WorkOrderNo, MaterialCode, Status, CreateBy)
    VALUES (1, N'WO-20260711-001', N'MAT-001', 1, 2);

IF NOT EXISTS (SELECT 1 FROM dbo.Production_PassRecord WHERE FactoryId = 1 AND Barcode = N'WF202607110001')
    INSERT INTO dbo.Production_PassRecord (FactoryId, WorkOrderNo, StationCode, Barcode, PassTime, CreateBy)
    VALUES (1, N'WO-20260711-001', N'ST-SCAN-01', N'WF202607110001', GETDATE(), 2);
GO

/* ========== 仓储入库单 ========== */
IF NOT EXISTS (SELECT 1 FROM dbo.Warehouse_InboundOrder WHERE FactoryId = 1 AND InboundNo = N'IN-20260711-001')
    INSERT INTO dbo.Warehouse_InboundOrder (FactoryId, InboundNo, Status, CreateBy)
    VALUES (1, N'IN-20260711-001', 1, 2);
GO

PRINT N'Business seed data completed';
PRINT N'  Barcode_Customer: 2';
PRINT N'  Barcode_MaterialRule: MAT-001(已确认) / MAT-002(待审核)';
PRINT N'  Sample barcodes: WF202607110001 ~ WF202607110003';
GO
