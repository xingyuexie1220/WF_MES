# WF MES

WF 制造执行系统（桌面客户端），面向工位 Windows 环境。

**数据访问定稿**

| 类别 | 方式 |
|------|------|
| 登录 / 选厂 / 切厂 / 改密 / 登出 / 桌面菜单 | **WF.MES.Api**（仅 Access Token，**无 Refresh**；过期须重新登录） |
| 条码及产线业务 | **SqlSugar 直连** `ConnectionStrings:WfMesDb` |
| 用户 / 角色 / 菜单维护 | **仅 Web 后台** |

用户/角色/菜单在 **Web 后台**维护；桌面心跳检测同端单设备互踢（后端 Redis 会话）。

## 解决方案结构

| 项目 | 说明 |
|------|------|
| `WF.MES.WPF` | WPF 客户端（Prism、HandyControl）。内部：`Auth/` / `Shell/` / `Ui/` / `Modules/{Mes,Material,Barcode,Equipment}` |
| `WF.MES.Infrastructure` | 业务 Service、SqlSugar、FluentValidation |
| `WF.MES.Core` | 接口、DTO、实体、常量 |
| `WF.MES.Updater` | 在线更新辅助程序 |

功能页目录须与 `System_Menu.Component` 前缀一致（如 `Barcode.Print` → `Modules/Barcode/`）。

## 文档索引

面向 **内部 IT** 与 **产线用户** 的交付文档位于 `WF.MES.WPF/Docs/`：

| 文档 | 对象 |
|------|------|
| [文档总览与 PDF 导出说明](WF.MES.WPF/Docs/README.md) | 全员 |
| [01 部署安装手册（内部 IT）](WF.MES.WPF/Docs/01_部署安装手册_内部IT.md) | IT |
| [02 用户操作手册（产线用户）](WF.MES.WPF/Docs/02_用户操作手册_产线用户.md) | 产线 |
| [03 运维与故障排查（内部 IT）](WF.MES.WPF/Docs/03_运维与故障排查_内部IT.md) | IT |
| [04 功能清单与验收用例](WF.MES.WPF/Docs/04_功能清单与验收用例.md) | IT / 验收 |
| [新模组开发流程](WF.MES.WPF/Docs/WF_MES_新模组开发流程.md) | 开发 |
| [Release Notes v1.0.1.2](WF.MES.WPF/Docs/ReleaseNotes_v1.0.1.2.md) | 全员 |

数据库脚本说明：`WF.MES.WPF/Scripts/README.md`

## 快速开始（IT）

1. 执行 [database/sql/](../database/README.md)（推荐 `00_rebuild_all.sql`）初始化 **MES** 库
2. 部署并启动 **WF.MES.Api**（工位可访问的 `Api:BaseUrl`）
3. 复制 `appsettings.example.json` 为 `appsettings.json`，配置 `Api:BaseUrl` 与 `ConnectionStrings:WfMesDb`
4. 安装 .NET 8 Desktop Runtime（**x86**）、BarTender（打印工位）
5. 运行 `WF.MES.WPF.exe`

默认账号：`admin/Admin@123`、`operator/Operator@123`（与 Web/API 一致，见 [database/README.md](../database/README.md)）。

## 多语言与多工厂

桌面端 UI 文案独立维护于 [WF.MES.WPF/i18n/](WF.MES.WPF/i18n/)（`JsonLocalizationService` 在运行时加载输出目录 `i18n/{locale}.json`）。API `messageCode` 与菜单 `I18nKey` 键名协议见仓库 [i18n/README.md](../i18n/README.md)。

| 能力 | 说明 |
|------|------|
| 语言 | **登录页**可切换 简体中文 / 繁體中文 / English，并写入 `%LocalAppData%/WF.MES/wf_locale.txt`；**主界面不再切换语言** |
| API 语言 | HTTP 请求自动携带 `Accept-Language`（取自当前 locale） |
| 多工厂 | 登录时若账号可访问多个工厂，会弹出选厂窗口 |

同步 API messageCode 到桌面 UI 包（可选，构建时会自动复制 UI 包）：

```powershell
./i18n/scripts/sync-desktop.ps1
```

业务模块（条码、MES、设备）页面标题、按钮、列头与提示信息均已接入同一套 i18n。

## 当前版本

客户端版本：**1.0.1.2**（见 `WF.MES.WPF.csproj` / `update/version.json`）
