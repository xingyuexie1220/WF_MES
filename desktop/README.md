# WF MES

WF 制造执行系统（桌面客户端），面向工位 Windows 环境。登录/菜单走 **Backend API**，条码等业务 **SqlSugar 直连 MES**；用户/角色/菜单在 **Web 后台**维护。

## 解决方案结构

| 项目 | 说明 |
|------|------|
| `WF.MES.WPF` | WPF 客户端（Prism、HandyControl） |
| `WF.MES.Infrastructure` | 业务 Service、SqlSugar、FluentValidation |
| `WF.MES.Core` | 接口、DTO、实体、常量 |
| `WF.MES.Updater` | 在线更新辅助程序 |

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

桌面端与 Web / 移动端共用 [i18n/messages/](../i18n/messages/) 文案（`JsonLocalizationService` 在运行时加载 `i18n/{locale}.json`）。

| 能力 | 说明 |
|------|------|
| 语言 | 登录页与主界面顶栏均可切换 **简体中文 / 繁體中文 / English**；选择会保存到 `%LocalAppData%/WF.MES/wf_locale.txt` |
| API 语言 | HTTP 请求自动携带 `Accept-Language`，菜单标题随语言刷新 |
| 多工厂 | 登录时若账号可访问多个工厂，会弹出选厂窗口；主界面顶栏 **切换工厂** 可在同地区工厂间切换（需重新加载菜单权限） |

同步共享文案到本地输出目录（可选，构建时会自动复制）：

```powershell
./i18n/scripts/sync-desktop.ps1
```

业务模块（条码、MES、设备）页面标题、按钮、列头与提示信息均已接入同一套 i18n；切换语言后主界面菜单与已打开模块文案会同步刷新。

## 当前版本

客户端版本：**1.0.1.2**（见 `WF.MES.WPF.csproj` / `update/version.json`）
