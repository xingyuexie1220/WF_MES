/*
  WF.MES - 案例种子数据（全新库 / 重建后执行）
  账号:
    admin    / Admin@123    系统管理员（Web + Mobile + Desktop 全权限）
    operator / Operator@123 车间操作员（Mobile + Desktop 生产菜单）
  密码哈希: PBKDF2-SHA256 100000 次（与后端 PasswordHasher 一致）
  MenuType: 1=目录 2=菜单 3=按钮
  ClientType: 1=Web 2=Mobile 3=Desktop
*/

USE [WF_MES_DEV];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;

/* ========== 地区 ========== */
SET IDENTITY_INSERT dbo.System_Region ON;
INSERT INTO dbo.System_Region (Id, RegionCode, RegionName, Sort, Status, CreateTime, IsDeleted)
VALUES (1, N'SOUTH-CN', N'华南地区', 1, 1, GETDATE(), 0);
SET IDENTITY_INSERT dbo.System_Region OFF;
GO

/* ========== 工厂 ========== */
SET IDENTITY_INSERT dbo.System_Factory ON;
INSERT INTO dbo.System_Factory (Id, RegionId, FactoryCode, FactoryName, TimeZone, Sort, Status, CreateTime, IsDeleted)
VALUES
    (1, 1, N'FAB-DG-A', N'WF东莞A厂', N'Asia/Shanghai', 1, 1, GETDATE(), 0),
    (2, 1, N'FAB-DG-B', N'WF东莞B厂', N'Asia/Shanghai', 2, 1, GETDATE(), 0);
SET IDENTITY_INSERT dbo.System_Factory OFF;
GO

/* ========== 部门（工厂内：车间/产线/班组） ========== */
SET IDENTITY_INSERT dbo.System_Dept ON;
INSERT INTO dbo.System_Dept (Id, FactoryId, ParentId, DeptCode, DeptName, DeptType, Sort, Status, CreateTime, IsDeleted)
VALUES
    (1,  1, 0, N'WF-A-M01',  N'A厂机加工一车间', 1, 1, 1, GETDATE(), 0),
    (2,  1, 1, N'WF-A-L01',  N'A厂产线1',        2, 1, 1, GETDATE(), 0),
    (3,  1, 2, N'WF-A-T01',  N'A厂班组1',        3, 1, 1, GETDATE(), 0),
    (4,  1, 0, N'WF-A-WH',   N'A厂仓储部',       1, 2, 1, GETDATE(), 0),
    (5,  2, 0, N'WF-B-M01',  N'B厂机加工一车间', 1, 1, 1, GETDATE(), 0);
SET IDENTITY_INSERT dbo.System_Dept OFF;
DBCC CHECKIDENT ('dbo.System_Dept', RESEED, 5);
GO

/* ========== 角色 ========== */
SET IDENTITY_INSERT dbo.System_Role ON;
INSERT INTO dbo.System_Role (Id, RoleCode, RoleName, Sort, DataScope, Status, Remark, CreateTime, IsDeleted)
VALUES
    (1, N'admin',    N'系统管理员', 1, 1, 1, N'Web 后台全权限', GETDATE(), 0),
    (2, N'operator', N'车间操作员', 2, 3, 1, N'Mobile + Desktop 生产端', GETDATE(), 0);
SET IDENTITY_INSERT dbo.System_Role OFF;
GO

/* ========== 菜单 ========== */
SET IDENTITY_INSERT dbo.System_Menu ON;
INSERT INTO dbo.System_Menu
    (Id, ParentId, MenuName, MenuType, [Path], Component, Permission, Icon, Sort, ClientType, Visible, Status, I18nKey, CreateTime, IsDeleted)
VALUES
    /* Web (ClientType=1) */
    (1,   0, N'系统设置', 1, N'/system',              NULL,                    NULL,                    N'Setting',        1, 1, 1, 1, N'menu.system',          GETDATE(), 0),
    (2,   1, N'组织架构', 2, N'/system/dept',          N'system/dept/index',    N'system:dept:list',    N'OfficeBuilding', 1, 1, 1, 1, N'menu.orgStructure',    GETDATE(), 0),
    (3,   1, N'用户管理', 2, N'/system/user',          N'system/user/index',    N'system:user:list',    N'User',           2, 1, 1, 1, N'menu.user',            GETDATE(), 0),
    (4,   1, N'菜单管理', 2, N'/system/menu',          N'system/menu/index',    N'system:menu:list',    N'Menu',           3, 1, 1, 1, N'menu.permission',      GETDATE(), 0),
    (5,   1, N'角色管理', 2, N'/system/role',          N'system/role/index',    N'system:role:list',    N'UserFilled',     4, 1, 1, 1, N'menu.role',            GETDATE(), 0),
    (6,   1, N'平台配置', 1, N'/system/platform',     NULL,                    NULL,                    N'Tools',          5, 1, 1, 1, N'menu.platformConfig',  GETDATE(), 0),
    (7,   1, N'系统监控', 1, N'/system/monitor',     NULL,                    NULL,                    N'Monitor',        6, 1, 1, 1, N'menu.systemMonitor',   GETDATE(), 0),
    (8,   6, N'数据字典', 2, N'/system/dict',         N'system/dict/index',    N'system:dict:list',    N'Collection',     1, 1, 1, 1, N'menu.dict',            GETDATE(), 0),
    (9,   6, N'消息公告', 2, N'/system/notice',       N'system/notice/index',  N'system:notice:list',  N'Bell',           2, 1, 1, 1, N'menu.notice',          GETDATE(), 0),
    (10,  7, N'定时任务', 2, N'/system/job',          N'system/job/index',     N'system:job:list',     N'Timer',          1, 1, 1, 1, N'menu.job',             GETDATE(), 0),
    (11,  7, N'系统日志', 2, N'/system/log',          N'system/log/index',     N'system:log:list',     N'Document',       2, 1, 1, 1, N'menu.systemLog',       GETDATE(), 0),
    (13,  7, N'在线会话', 2, N'/system/session',      N'system/session/index', N'system:session:list', N'Connection',     3, 1, 1, 1, N'menu.session',         GETDATE(), 0),
    (12,  1, N'岗位管理', 2, N'/system/position',     N'system/position/index',N'system:position:list',N'Briefcase',      7, 1, 1, 1, N'menu.position',        GETDATE(), 0),
    (14,  1, N'工厂管理', 2, N'/system/factory',      N'system/factory/index', N'system:factory:list', N'OfficeBuilding', 8, 1, 1, 1, N'menu.factory',         GETDATE(), 0),
    (110, 2, N'组织新增', 3, NULL, NULL, N'system:dept:add',     NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (111, 2, N'组织编辑', 3, NULL, NULL, N'system:dept:edit',    NULL, 2, 1, 0, 1, NULL, GETDATE(), 0),
    (112, 2, N'组织删除', 3, NULL, NULL, N'system:dept:delete',  NULL, 3, 1, 0, 1, NULL, GETDATE(), 0),
    (101, 3, N'用户新增', 3, NULL, NULL, N'system:user:add',     NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (102, 3, N'用户编辑', 3, NULL, NULL, N'system:user:edit',    NULL, 2, 1, 0, 1, NULL, GETDATE(), 0),
    (103, 3, N'用户删除', 3, NULL, NULL, N'system:user:delete',  NULL, 3, 1, 0, 1, NULL, GETDATE(), 0),
    (107, 4, N'菜单新增', 3, NULL, NULL, N'system:menu:add',     NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (108, 4, N'菜单编辑', 3, NULL, NULL, N'system:menu:edit',    NULL, 2, 1, 0, 1, NULL, GETDATE(), 0),
    (109, 4, N'菜单删除', 3, NULL, NULL, N'system:menu:delete',  NULL, 3, 1, 0, 1, NULL, GETDATE(), 0),
    (104, 5, N'角色新增', 3, NULL, NULL, N'system:role:add',     NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (105, 5, N'角色编辑', 3, NULL, NULL, N'system:role:edit',    NULL, 2, 1, 0, 1, NULL, GETDATE(), 0),
    (106, 5, N'角色删除', 3, NULL, NULL, N'system:role:delete',  NULL, 3, 1, 0, 1, NULL, GETDATE(), 0),
    (121, 8, N'字典新增', 3, NULL, NULL, N'system:dict:add',     NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (122, 8, N'字典编辑', 3, NULL, NULL, N'system:dict:edit',    NULL, 2, 1, 0, 1, NULL, GETDATE(), 0),
    (123, 8, N'字典删除', 3, NULL, NULL, N'system:dict:delete',  NULL, 3, 1, 0, 1, NULL, GETDATE(), 0),
    (124, 9, N'公告新增', 3, NULL, NULL, N'system:notice:add',   NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (125, 9, N'公告编辑', 3, NULL, NULL, N'system:notice:edit',  NULL, 2, 1, 0, 1, NULL, GETDATE(), 0),
    (126, 9, N'公告删除', 3, NULL, NULL, N'system:notice:delete',NULL, 3, 1, 0, 1, NULL, GETDATE(), 0),
    (127, 10, N'任务新增', 3, NULL, NULL, N'system:job:add',      NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (128, 10, N'任务编辑', 3, NULL, NULL, N'system:job:edit',     NULL, 2, 1, 0, 1, NULL, GETDATE(), 0),
    (129, 10, N'任务删除', 3, NULL, NULL, N'system:job:delete',   NULL, 3, 1, 0, 1, NULL, GETDATE(), 0),
    (130, 10, N'任务执行', 3, NULL, NULL, N'system:job:run',      NULL, 4, 1, 0, 1, NULL, GETDATE(), 0),
    (131, 11, N'日志导出', 3, NULL, NULL, N'system:log:export',   NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (132, 11, N'日志清空', 3, NULL, NULL, N'system:log:clear',    NULL, 2, 1, 0, 1, NULL, GETDATE(), 0),
    (133, 13, N'强制下线', 3, NULL, NULL, N'system:session:kick', NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (113, 12, N'岗位新增', 3, NULL, NULL, N'system:position:add',    NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (114, 12, N'岗位编辑', 3, NULL, NULL, N'system:position:edit',   NULL, 2, 1, 0, 1, NULL, GETDATE(), 0),
    (115, 12, N'岗位删除', 3, NULL, NULL, N'system:position:delete', NULL, 3, 1, 0, 1, NULL, GETDATE(), 0),
    (134, 14, N'工厂新增', 3, NULL, NULL, N'system:factory:add',    NULL, 1, 1, 0, 1, NULL, GETDATE(), 0),
    (135, 14, N'工厂编辑', 3, NULL, NULL, N'system:factory:edit',   NULL, 2, 1, 0, 1, NULL, GETDATE(), 0),
    (136, 14, N'工厂删除', 3, NULL, NULL, N'system:factory:delete', NULL, 3, 1, 0, 1, NULL, GETDATE(), 0),

    /* Web 生产配置/报表/大屏 (400-499) */
    (400, 0,   N'生产配置', 1, N'/master-data',           NULL,                         NULL,                         N'Setting',   2, 1, 1, 1, N'menu.masterData',      GETDATE(), 0),
    (401, 400, N'物料管理', 2, N'/master-data/material',  N'master-data/material/index', N'master:material:list',      N'Box',       1, 1, 1, 1, N'menu.masterMaterial',  GETDATE(), 0),
    (402, 400, N'工艺路线', 2, N'/master-data/route',     N'master-data/route/index',    N'master:route:list',         N'Guide',     2, 1, 1, 1, N'menu.masterRoute',     GETDATE(), 0),
    (403, 400, N'工站配置', 2, N'/master-data/station',   N'master-data/station/index',  N'master:station:list',       N'Location',  3, 1, 1, 1, N'menu.masterStation',   GETDATE(), 0),
    (404, 400, N'工作中心', 2, N'/master-data/workcenter',N'master-data/workcenter/index',N'master:workcenter:list',  N'OfficeBuilding',4,1,1,1,N'menu.masterWorkCenter',GETDATE(), 0),
    (410, 0,   N'生产报表', 1, N'/report',                NULL,                         NULL,                         N'DataLine',  3, 1, 1, 1, N'menu.report',          GETDATE(), 0),
    (411, 410, N'产量报表', 2, N'/report/output',         N'report/output/index',        N'dashboard:report:view',     N'Histogram', 1, 1, 1, 1, N'menu.reportOutput',    GETDATE(), 0),
    (412, 410, N'WIP报表',  2, N'/report/wip',            N'report/wip/index',           N'dashboard:report:view',     N'PieChart',  2, 1, 1, 1, N'menu.reportWip',       GETDATE(), 0),
    (420, 0,   N'数据大屏', 2, N'/bigscreen',             N'bigscreen/index',            N'dashboard:bigscreen:view',  N'Monitor',   4, 1, 1, 1, N'menu.bigscreen',       GETDATE(), 0),

    /* Mobile (ClientType=2) */
    (200, 0,   N'手机端 MES', 1, N'/mobile',               NULL,                       NULL,                              N'phone',  1, 2, 1, 1, N'menu.mobile.root',       GETDATE(), 0),
    (201, 200, N'工作台',     2, N'/pages/home/index',     N'pages/home/index',         N'mobile:home:list',              N'home',   1, 2, 1, 1, N'menu.mobile.home',         GETDATE(), 0),
    (202, 200, N'扫码入库',   2, N'/pages/warehouse/scan', N'pages/warehouse/scan',     N'mobile:warehouse:scan:list',    N'scan',   2, 2, 1, 1, N'menu.mobile.warehouseScan',GETDATE(), 0),
    (203, 200, N'库存查询',   2, N'/pages/inventory/list', N'pages/inventory/list',   N'mobile:inventory:list',         N'list',   3, 2, 1, 1, N'menu.mobile.inventory',    GETDATE(), 0),
    (204, 202, N'提交扫码',   3, NULL, NULL, N'mobile:warehouse:scan:submit', NULL, 1, 2, 0, 1, NULL, GETDATE(), 0),
    (205, 202, N'取消扫码',   3, NULL, NULL, N'mobile:warehouse:scan:cancel', NULL, 2, 2, 0, 1, NULL, GETDATE(), 0),
    (206, 200, N'简单过站',   2, N'/pages/mes/simple-pass', N'pages/mes/simple-pass', N'mobile:mes:pass:list', N'Finished', 4, 2, 1, 1, N'menu.mobile.simplePass', GETDATE(), 0),

    /* Desktop / WPF (ClientType=3) — Component = Prism RegisterForNavigation 名称 */
    (300, 0,   N'桌面端',     1, N'/desktop',                   NULL,                    NULL,                      N'Monitor',      1, 3, 1, 1, N'menu.desktop.root',             GETDATE(), 0),
    (301, 300, N'生产执行',   1, N'/desktop/mes',               NULL,                    NULL,                      N'Operation',    1, 3, 1, 1, N'menu.desktop.mes',              GETDATE(), 0),
    (302, 301, N'工单扫码',   2, N'/desktop/mes/workorder',     N'Mes.WorkOrderScan',    N'mes:workorder:scan',     N'Scan',         1, 3, 1, 1, N'menu.desktop.workorder',        GETDATE(), 0),
    (303, 301, N'箱体返工',   2, N'/desktop/mes/rework',        N'Mes.BoxRework',        N'mes:box:rework',         N'Refresh',      2, 3, 1, 1, N'menu.desktop.rework',           GETDATE(), 0),
    (304, 301, N'物料维护',   2, N'/desktop/material/maintain', N'Material.Maintain',    N'material:maintain:list', N'Edit',         3, 3, 1, 1, N'menu.desktop.materialMaintain', GETDATE(), 0),
    (305, 301, N'物料扫码',   2, N'/desktop/material/scan',     N'Material.Scan',        N'material:scan:list',     N'Camera',       4, 3, 1, 1, N'menu.desktop.materialScan',     GETDATE(), 0),
    (310, 300, N'条码管理',   1, N'/desktop/barcode',           NULL,                    NULL,                      N'Printer',      2, 3, 1, 1, N'menu.desktop.barcode',          GETDATE(), 0),
    (311, 310, N'客户管理',   2, N'/desktop/barcode/customer',  N'Barcode.Customer',     N'barcode:customer:list',  N'User',         1, 3, 1, 1, N'menu.desktop.customer',         GETDATE(), 0),
    (312, 310, N'物料规则',   2, N'/desktop/barcode/rule',      N'Barcode.MaterialRule', N'barcode:rule:list',      N'Document',     2, 3, 1, 1, N'menu.desktop.materialRule',     GETDATE(), 0),
    (313, 310, N'条码打印',   2, N'/desktop/barcode/print',     N'Barcode.Print',        N'barcode:print:list',     N'Printer',      3, 3, 1, 1, N'menu.desktop.print',            GETDATE(), 0),
    (314, 310, N'条码明细',   2, N'/desktop/barcode/detail',    N'Barcode.Detail',       N'barcode:detail:list',    N'List',         4, 3, 1, 1, N'menu.desktop.detail',           GETDATE(), 0),
    (315, 310, N'条码补印',   2, N'/desktop/barcode/reprint',   N'Barcode.Reprint',      N'barcode:reprint:list',   N'CopyDocument', 5, 3, 1, 1, N'menu.desktop.reprint',          GETDATE(), 0),
    (316, 310, N'条码资料审核', 2, N'/desktop/barcode/qareview', N'Barcode.QaReview',     N'barcode:qareview:list',  N'DocumentChecked', 6, 3, 1, 1, N'menu.desktop.qaReview',         GETDATE(), 0),
    (317, 316, N'保存附件',   3, NULL, NULL, N'barcode:qareview:saveattachments', NULL, 1, 3, 0, 1, NULL, GETDATE(), 0),
    (318, 316, N'审核',       3, NULL, NULL, N'barcode:qareview:review',           NULL, 2, 3, 0, 1, NULL, GETDATE(), 0),
    (319, 301, N'组装作业',   2, N'/desktop/mes/assembly',     N'Mes.Assembly',     N'mes:assembly:list',      N'SetUp',        5, 3, 1, 1, N'menu.desktop.assembly',     GETDATE(), 0),
    (320, 300, N'设备测试',   1, N'/desktop/equipment',        NULL,                NULL,                      N'Cpu',          3, 3, 1, 1, N'menu.desktop.equipment',    GETDATE(), 0),
    (321, 320, N'测试对接',   2, N'/desktop/equipment/test',     N'Equipment.Test',   N'equipment:test:submit',  N'Connection',   1, 3, 1, 1, N'menu.desktop.equipmentTest',GETDATE(), 0);
SET IDENTITY_INSERT dbo.System_Menu OFF;
DBCC CHECKIDENT ('dbo.System_Menu', RESEED, 421);
GO

/* ========== 角色菜单 ========== */
INSERT INTO dbo.System_Role_Menu (RoleId, MenuId)
SELECT 1, Id FROM dbo.System_Menu WHERE IsDeleted = 0;

INSERT INTO dbo.System_Role_Menu (RoleId, MenuId)
SELECT 2, Id FROM dbo.System_Menu
WHERE IsDeleted = 0
  AND Id IN (200, 201, 202, 203, 204, 205, 206, 300, 301, 302, 303, 304, 305, 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321);
GO

/* ========== 岗位 ========== */
SET IDENTITY_INSERT dbo.System_Position ON;
INSERT INTO dbo.System_Position (Id, PositionCode, PositionName, ProcessCode, DeptId, Sort, Status, CreateTime, IsDeleted)
VALUES (1, N'OP-001', N'报工员', N'PROC-REPORT', 1, 1, 1, GETDATE(), 0);
SET IDENTITY_INSERT dbo.System_Position OFF;
GO

/* ========== 用户 ========== */
SET IDENTITY_INSERT dbo.[System_User] ON;
INSERT INTO dbo.[System_User] (Id, UserName, PasswordHash, NickName, DeptId, DefaultFactoryId, Status, MustChangePassword, CreateTime, IsDeleted)
VALUES
    (1, N'admin',    N'WHrWZ1xysjyC07VVYvq/MA==.tl/J7JvJsvoGFNlrZ+wa/vGixz72EZHVs5IfJIe74xU=', N'超级管理员',     1, 1, 1, 0, GETDATE(), 0),
    (2, N'operator', N'1UGdzqaud8R1Yl1klTtzsg==.IIJrXx/BBFgvsMoJdQryYkIWid7IAFJQ4Zpf0B7Tz3o=', N'东莞A厂操作员', 3, 1, 1, 1, GETDATE(), 0);
SET IDENTITY_INSERT dbo.[System_User] OFF;
GO

INSERT INTO dbo.System_User_Role (UserId, RoleId) VALUES (1, 1), (2, 2);
INSERT INTO dbo.System_User_Position (UserId, PositionId) VALUES (2, 1);
INSERT INTO dbo.System_User_Factory (UserId, FactoryId, IsDefault) VALUES
    (1, 1, 1), (1, 2, 0),
    (2, 1, 1);
GO

/* ========== 数据字典 ========== */
SET IDENTITY_INSERT dbo.System_Dict_Type ON;
INSERT INTO dbo.System_Dict_Type (Id, DictName, DictType, Status, Remark, CreateTime, IsDeleted)
VALUES
    (1, N'公告类型', N'sys_notice_type',   1, N'消息公告分类', GETDATE(), 0),
    (2, N'公告状态', N'sys_notice_status', 1, N'消息公告发布状态', GETDATE(), 0);
SET IDENTITY_INSERT dbo.System_Dict_Type OFF;

INSERT INTO dbo.System_Dict_Data (DictTypeId, DictType, DictLabel, DictValue, Sort, Status, CreateTime, IsDeleted)
VALUES
    (1, N'sys_notice_type',   N'通知',   N'1', 1, 1, GETDATE(), 0),
    (1, N'sys_notice_type',   N'公告',   N'2', 2, 1, GETDATE(), 0),
    (2, N'sys_notice_status', N'草稿',   N'0', 1, 1, GETDATE(), 0),
    (2, N'sys_notice_status', N'已发布', N'1', 2, 1, GETDATE(), 0);
GO

/* ========== 案例公告 ========== */
INSERT INTO dbo.System_Notice (Title, Content, NoticeType, Status, PublishTime, CreateBy, CreateTime, IsDeleted)
VALUES
    (N'系统上线通知', N'WF MES 平台已部署完成，请各工位使用 operator 账号登录桌面端与移动端。', 1, 1, SYSDATETIME(), 1, SYSDATETIME(), 0);
GO

PRINT N'Seed data completed';
PRINT N'  admin    / Admin@123';
PRINT N'  operator / Operator@123';
GO
