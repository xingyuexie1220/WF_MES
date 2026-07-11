# 桌面端脚本（MES 库）

> 建表、种子数据、菜单权限请使用仓库根目录 **[database/sql/](../../database/README.md)**。  
> 用户/角色/菜单在 **Web 后台** 维护；桌面端通过 **API** 登录并拉取菜单。

## 本目录内容

| 路径 | 说明 |
|------|------|
| `Barcode_PurgeRetention.sql` | 条码明细保留期清理存储过程（在 MES 库执行） |
| `scheduled-tasks/` | Windows 计划任务示例（定时执行清理） |

## 配置说明（appsettings.json）

| 节点 | 说明 |
|------|------|
| `Api:BaseUrl` | 后端 API 地址（登录/菜单/改密）。HTTP 常用 `:5088`，HTTPS 常用 `:7143` |
| `ConnectionStrings:WfMesDb` | 条码等业务 SqlSugar 直连 MES（`Database=MES`） |
| `Session:HeartbeatIntervalSeconds` | 调用 `/auth/info` 校验会话间隔（秒） |

**Debug 构建** 叠加 `appsettings.Development.json`（默认 `http://localhost:5088`）。  
**Release 构建** 叠加 `appsettings.Production.json`（现网 API 地址，发布前请核对）。

## NMES 历史库迁移

见 [database/README.md](../../database/README.md) → **NMES 迁移到 MES**：

1. `02_create_tables.sql` 或 `00_rebuild_all.sql`
2. `24_migrate_nmes_barcode_data.sql`（如有 NMES 条码数据）
3. 确认 `appsettings.json` 中 `Database=MES` 且 `Api:BaseUrl` 指向已部署的 API
