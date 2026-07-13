# WF.MES 数据库脚本

## 说明

数据库结构及种子数据 **统一通过 SQL 脚本维护**，后端启动时 **不会** 自动建表或写入种子数据。

实体类上的 `[SugarTable]` 仅用于 SqlSugar ORM 映射已有表，不会同步表结构。

**`02_create_tables.sql` 与 `docs/MES.dbo.initdefinition.md`（你的建表定义）一致**，共约 30 张表：`System_*` + `Barcode_*` + `Master_*` / `Production_*` / `Warehouse_*`。

**新环境推荐**：使用 `00_rebuild_all.sql` 一键清空重建（不保留历史数据）。

**权威参考**：`database/docs/MES.dbo.initdefinition.md`（由桌面 `MES.dbo.initdefinition.md` 同步）

## 脚本清单

| 文件 | 说明 |
|------|------|
| `sql/00_init_all.sql` | 全新库初始化（01 → 02 → 03，不删表） |
| `sql/00_rebuild_all.sql` | **推荐** 清空重建（01 → 20 → 02 → 03） |
| `sql/01_create_database.sql` | 创建数据库 `MES` |
| `sql/02_create_tables.sql` | 创建全部业务表 |
| `sql/03_seed_data.sql` | 三端菜单 + 角色授权 + 部门/用户/字典/公告种子 |
| `sql/04_seed_business_data.sql` | 条码/主数据/生产/仓储业务测试数据 |
| `sql/05_upgrade_multifactory.sql` | **已有库增量升级**（补 `DefaultFactoryId`、多工厂表） |
| `sql/06_rename_table_prefixes.sql` | **已有库**表前缀重命名（缩写 → 全词） |
| `sql/20_drop_all_tables.sql` | 删除全部业务表（重建前执行） |
| `sql/24_migrate_nmes_barcode_data.sql` | **NMES → MES** 条码数据迁移（一次性） |

## 表命名规范

系统表与条码表统一 **PascalCase + 下划线**：

| 前缀 | 含义 | 示例 |
|------|------|------|
| `System_` | 系统平台 | `System_User`、`System_Role_Menu`、`System_Factory` |
| `Barcode_` | 条码业务 | `Barcode_Customer`、`Barcode_MaterialRule`、`Barcode_Record` |
| `Master_` | 主数据 | `Master_Material`、`Master_Route`、`Master_Station` |
| `Production_` | 生产执行 | `Production_WorkOrder`、`Production_PassRecord` |
| `Warehouse_` | 仓储 | `Warehouse_InboundOrder` |

字典 **编码**（如 `System_notice_type`）为业务字段值，不是表名，保持小写不变。

## NMES 迁移到 MES（桌面客户端历史库）

旧桌面客户端使用独立库 **NMES**（`System_*` + `Barcode_*`）。统一平台后 **全部业务落在 MES**：

| 数据类型 | 处理方式 |
|----------|----------|
| 用户/角色/菜单/会话 | 使用 MES `System_*`，**不**从 NMES 迁移；Web 后台维护 |
| 条码业务（客户/规则/生成单/明细等） | 先确保 MES 有表结构（`02` 或 `00_rebuild_all`），再执行 `24` |
| 桌面 WPF 连接串 | `appsettings.json` → `Database=MES` |

**推荐步骤（现网已有 NMES 数据）：**

```bash
# 1. 备份
# BACKUP DATABASE MES / NMES ...

# 2. MES 补全表结构（若尚无 Barcode_* 表）
sqlcmd ... -i 02_create_tables.sql
# 或全新重建：sqlcmd ... -i 00_rebuild_all.sql

# 3. 迁移条码数据
sqlcmd ... -i 24_migrate_nmes_barcode_data.sql

# 4. 修改 desktop appsettings.json：Database=MES
```

**全新环境**：直接 `00_rebuild_all.sql`（含全部表 + 种子数据，含 QaReview 菜单、MustChangePassword 等）。

## 执行方式

### 全新部署 / 清空重建（推荐）

```bash
cd database/sql
sqlcmd -S localhost,1433 -U sa -P "WfMes@123456" -i 00_rebuild_all.sql
```

### 仅首次建库（表不存在时）

```bash
cd database/sql
sqlcmd -S localhost,1433 -U sa -P "WfMes@123456" -i 00_init_all.sql
```

### SSMS 手动执行

重建：`01` → `20` → `02` → `03`  
全新：`01` → `02` → `03`

## 默认账号

执行 `03_seed_data.sql` 后可直接使用：

| 账号 | 密码 | 权限 |
|------|------|------|
| admin | Admin@123 | Web + Mobile + Desktop 全菜单 |
| operator | Operator@123 | Mobile + Desktop 生产菜单 |

如需重新生成密码哈希：

```bash
cd database/tools/GenHash
dotnet run
```

将输出结果更新到 `03_seed_data.sql` 中对应用户的 `PasswordHash` 字段。

## 三端菜单 ID 规划

| ClientType | ID 段 | 说明 |
|------------|-------|------|
| 1 Web | 1–199 | Vue 组件路径如 `system/user/index` |
| 2 Mobile | 200–299 | uni-app 页面如 `pages/home/index` |
| 3 Desktop | 300–399 | WPF Prism 导航名如 `Mes.WorkOrderScan` |

## 连接字符串

与 `backend/WF.MES.Api/appsettings.json` 保持一致：

```
Server=localhost,1433;Database=MES;User Id=sa;Password=WfMes@123456;TrustServerCertificate=True;
```

## 表结构变更流程

1. 修改 `database/sql/02_create_tables.sql` 与 `03_seed_data.sql`
2. 在测试库执行 `00_rebuild_all.sql` 并验证
3. 同步更新 `backend/WF.MES.Domain/Entities/` 实体类字段
