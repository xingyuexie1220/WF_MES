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
| `ConnectionStrings:WfMesDb` | 条码等业务 SqlSugar 直连（与现网 `Database` 名一致） |
| `Session:HeartbeatIntervalSeconds` | 调用 `/auth/info` 校验会话间隔（秒） |

**Debug 构建** 叠加 `appsettings.Development.json`（默认 `http://localhost:5088`）。  
**Release 构建** 叠加 `appsettings.Production.json`（现网 API 地址，发布前请核对）。

建库与种子请走仓库 `database/sql/00_rebuild_all.sql`（或 `00_init_all.sql`）；桌面端**不维护**旧库迁移路径。
