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
/* 规则1: WF + YYYY + 4位流水，已 QA 确认，可打印。示例条码: WF20260001（日期格式码为单段 YYYY，非 yyyyMMdd） */
IF NOT EXISTS (SELECT 1 FROM dbo.Barcode_MaterialRule WHERE Factory_Id = 1 AND Material_No = N'MAT-001')
BEGIN
    INSERT INTO dbo.Barcode_MaterialRule
        (Factory_Id, Customer_Id, Material_No, Barcode_Length, Qa_Status, Qa_Reviewed_By, Qa_Reviewed_At, Qa_Review_Remark, CreatedBy)
    VALUES
        (1, @CustomerHuawei, N'MAT-001', 10, 2, N'admin', GETDATE(), N'测试数据-已确认', N'admin');

    DECLARE @Rule1 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Barcode_RuleSegment (Rule_Id, Sort_Order, Segment_Type, Config_Json, Include_In_ResetKey)
    VALUES
        (@Rule1, 1, N'Literal', N'{"value":"WF"}', 1),
        (@Rule1, 2, N'Date',    N'{"format":"YYYY"}', 1),
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
        (1, @CustomerFoxconn2, N'MAT-002', 10, 1, N'operator');

    DECLARE @Rule2 INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Barcode_RuleSegment (Rule_Id, Sort_Order, Segment_Type, Config_Json, Include_In_ResetKey)
    VALUES
        (@Rule2, 1, N'Literal', N'{"value":"FX"}', 1),
        (@Rule2, 2, N'Date',    N'{"format":"YYYY"}', 1),
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
        (1, N'GEN-20260711-001', @Rule1Id, N'MAT-001', N'WF|2026', CAST(GETDATE() AS DATE), 3, 1, 3, 1, N'operator');

    DECLARE @GenId INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Barcode_Record (Factory_Id, Generate_Record_Id, Rule_Id, Barcode, Reset_Key, Serial_Value, Status)
    VALUES
        (1, @GenId, @Rule1Id, N'WF20260001', N'WF|2026', 1, 1),
        (1, @GenId, @Rule1Id, N'WF20260002', N'WF|2026', 2, 1),
        (1, @GenId, @Rule1Id, N'WF20260003', N'WF|2026', 3, 1);

    INSERT INTO dbo.Barcode_SerialCounter (Rule_Id, Reset_Key, Current_Value)
    VALUES (@Rule1Id, N'WF|2026', 3);
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

/* ========== Mes_* 简易报工演示数据 ========== */
IF NOT EXISTS (SELECT 1 FROM dbo.Mes_Process WHERE FactoryId = 1 AND ProcessCode = N'P10')
BEGIN
    INSERT INTO dbo.Mes_Process (FactoryId, ProcessCode, ProcessName, DefaultSeq, Enabled, CreateBy)
    VALUES
        (1, N'P10', N'领料投料',   10, 1, 1),
        (1, N'P20', N'CNC加工',    20, 1, 1),
        (1, N'P30', N'去毛刺抛光', 30, 1, 1),
        (1, N'P40', N'检验',       40, 1, 1),
        (1, N'P50', N'入库',       50, 1, 1);
END
GO

DECLARE @MesRoutingId BIGINT;

IF NOT EXISTS (SELECT 1 FROM dbo.Mes_Routing WHERE FactoryId = 1 AND RoutingCode = N'STD-HS')
BEGIN
    INSERT INTO dbo.Mes_Routing (FactoryId, RoutingCode, RoutingName, MaterialNo, Enabled, Remark, CreateBy)
    VALUES (1, N'STD-HS', N'散热器外壳标准工艺', N'GPU-HS-AL-001', 1, N'CNC外壳演示工艺', 1);

    SET @MesRoutingId = SCOPE_IDENTITY();

    INSERT INTO dbo.Mes_Routing_Step (FactoryId, RoutingId, ProcessCode, Seq, CreateBy)
    VALUES
        (1, @MesRoutingId, N'P10', 10, 1),
        (1, @MesRoutingId, N'P20', 20, 1),
        (1, @MesRoutingId, N'P30', 30, 1),
        (1, @MesRoutingId, N'P40', 40, 1),
        (1, @MesRoutingId, N'P50', 50, 1);
END
ELSE
    SET @MesRoutingId = (SELECT Id FROM dbo.Mes_Routing WHERE FactoryId = 1 AND RoutingCode = N'STD-HS' AND IsDeleted = 0);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Mes_Material WHERE FactoryId = 1 AND MaterialNo = N'GPU-HS-AL-001')
    INSERT INTO dbo.Mes_Material (FactoryId, MaterialNo, MaterialName, Spec, Unit, Source, Enabled, CreateBy)
    VALUES (1, N'GPU-HS-AL-001', N'显卡散热器金属外壳', N'铝合金 CNC', N'PCS', N'local', 1, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Mes_Machine WHERE FactoryId = 1 AND MachineNo = N'CNC-01')
    INSERT INTO dbo.Mes_Machine (FactoryId, MachineNo, MachineName, Enabled, CreateBy)
    VALUES
        (1, N'CNC-01', N'CNC一号机', 1, 1),
        (1, N'CNC-02', N'CNC二号机', 1, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Mes_Defect_Code WHERE FactoryId = 1 AND DefectCode = N'SIZE')
    INSERT INTO dbo.Mes_Defect_Code (FactoryId, DefectCode, DefectName, Sort, Enabled, CreateBy)
    VALUES
        (1, N'SIZE',    N'尺寸超差',   1, 1, 1),
        (1, N'SCRATCH', N'外观划伤',   2, 1, 1),
        (1, N'TOOL',    N'缺料/撞刀',  3, 1, 1),
        (1, N'THREAD',  N'螺纹不良',   4, 1, 1),
        (1, N'OTHER',   N'其他',       9, 1, 1);
GO

DECLARE @MesRoutingId2 BIGINT = (SELECT TOP 1 Id FROM dbo.Mes_Routing WHERE FactoryId = 1 AND RoutingCode = N'STD-HS' AND IsDeleted = 0);

IF @MesRoutingId2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Mes_Work_Order WHERE FactoryId = 1 AND WorkOrderNo = N'WO202607130001')
BEGIN
    INSERT INTO dbo.Mes_Work_Order
        (FactoryId, WorkOrderNo, MaterialNo, RoutingId, PlanQty, DueDate, Status, Source, SyncedAt, Remark, CreateBy)
    VALUES
        (1, N'WO202607130001', N'GPU-HS-AL-001', @MesRoutingId2, 100, DATEADD(DAY, 7, CAST(GETDATE() AS DATE)), N'open', N'local', GETDATE(), N'演示工单-可直接 PDA 扫码报工', 1);

    INSERT INTO dbo.Mes_Report_Record
        (FactoryId, WorkOrderNo, ProcessCode, GoodQty, DefectQty, OperatorName, ReportTime, CreateBy)
    VALUES
        (1, N'WO202607130001', N'P10', 100, 0, N'admin', GETDATE(), 1);
END
GO

PRINT N'Business seed data completed';
PRINT N'  Barcode_Customer: 2';
PRINT N'  Barcode_MaterialRule: MAT-001(已确认) / MAT-002(待审核)';
PRINT N'  Sample barcodes: WF202607110001 ~ WF202607110003';
PRINT N'  Mes demo work order: WO202607130001 (plan 100, P10 already reported 100)';
GO
