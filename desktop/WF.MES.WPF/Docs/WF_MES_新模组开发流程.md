# WF MES 新模组开发流程

> 适用架构：WF.MES.Core / WF.MES.Infrastructure / WF.MES.WPF  
> 文档版本：2026-07-07  
> 核心原则：**ViewModel 只依赖 Core 接口，业务逻辑在 Infrastructure，WPF 只管 UI 和导航**

---

## 1. 分层职责

| 内容 | 项目 | 路径示例 |
|------|------|----------|
| 接口、常量、DTO、Entity | WF.MES.Core | `Core/Interfaces/`、`Models/Dtos/` |
| Service 实现、SqlSugar、Validator | WF.MES.Infrastructure | `Services/`、`Validation/` |
| View、ViewModel、弹窗 | WF.MES.WPF | `Modules/{模组}/Views/`、`ViewModels/` |
| 菜单/权限 | Web 后台 + `database/sql/03_seed_data.sql` |

**依赖方向（单向）：**

```
WF.MES.WPF  →  WF.MES.Infrastructure  →  WF.MES.Core
```

ViewModel **禁止**引用 `WF.MES.Infrastructure.*`（包括 static helper、SqlSugar、Validator 具体类）。

---

## 2. 新模组开发步骤（推荐顺序）

### 第 1 步：数据库 + Core 模型

1. 在 [database/sql/](../../../database/README.md) 的 `02_create_tables.sql` 增加业务表（或增量脚本）
2. **Entity** → `WF.MES.Core/Models/Entities/`
3. **DTO**（列表/编辑/查询）→ `WF.MES.Core/Models/Dtos/`
4. 业务常量 → `WF.MES.Core/Core/Constants/`

### 第 2 步：Core 定义接口

在 `WF.MES.Core/Core/Interfaces/` 新增或扩展接口，例如：

```csharp
public interface IWorkOrderService
{
    Task<IReadOnlyList<WorkOrderListDto>> GetListAsync(...);
    Task SaveAsync(WorkOrderEditDto dto, CancellationToken ct = default);
}
```

ViewModel **只引用这些接口**，不引用 Infrastructure。

### 第 3 步：Infrastructure 实现

1. **Service** → `WF.MES.Infrastructure/Services/`（可按模组分子目录）
2. Save/Generate 入口调用 `_validator.ValidateRequestAsync(dto)`
3. 新增 **Validator** → `WF.MES.Infrastructure/Validation/{模组}/`
4. 在 `InfrastructureServiceRegistration.cs` 注册 Service 与 Validator

**注册示例：**

```csharp
containerRegistry.RegisterSingleton<IWorkOrderService, WorkOrderService>();
containerRegistry.Register<IValidator<WorkOrderEditDto>, WorkOrderEditValidator>();
```

### 第 4 步：WPF 界面

目录结构：

```
WF.MES.WPF/Modules/{模组名}/
├── Views/
│   └── XxxView.xaml + .xaml.cs
└── ViewModels/
    └── XxxViewModel.cs
```

ViewModel 规范（参考 `CustomerManageViewModel`）：

- 构造函数 **只注入 Core 接口**（如 `ICustomerService`、`ISessionService`）
- 列表页实现 `INavigationAware`，在 `OnNavigatedTo` 加载数据
- 不 `using WF.MES.Infrastructure.*`
- 不直接 `new Service()`、不注入 `ISqlSugarClient`
- 操作人使用 `_sessionService.CurrentOperatorName`
- 打印使用 `_printService.CreatePrintRequest(...)`，不直接调用 Infrastructure 工具类

### 第 5 步：Prism 导航注册

在 `App.xaml.cs` → `RegisterModuleViews` 增加：

```csharp
containerRegistry.RegisterForNavigation<WorkOrderScanView, WorkOrderScanViewModel>("Mes.WorkOrderScan");
```

**Component 必须与 `RegisterForNavigation` 名称完全一致**（对应 `System_Menu.Component`，Web 菜单管理维护）。

### 第 6 步：Web 菜单 + 角色授权

1. 在 `database/sql/03_seed_data.sql`（或 Web **菜单管理**）增加桌面端菜单：`ClientType=3`，`Component` 与第 5 步一致
2. 在 Web **角色管理** 为目标角色勾选菜单（含父级目录）
3. 用户重新登录桌面端，侧栏自动显示（`GET /auth/desktop-menus`）

### 第 6.1 步：按钮权限

| 层级 | 配置位置 | 含义 |
|------|----------|------|
| 菜单 | `System_Menu` MenuType=2 + `System_Role_Menu` | 能否进入页面 |
| 按钮 | `System_Menu` MenuType=3 的 `Permission` 字段 | API 返回的权限码，桌面 `MenuActions` + `HasAction` |

**新增受控按钮 checklist：**

1. 在 `03_seed_data.sql` 增加 MenuType=3 按钮菜单，`Permission` 如 `barcode:xxx:action`
2. `MenuActions.cs` 增加同名常量
3. ViewModel `CanXxx => _auth.HasAction(...)`
4. Service 写入口：`EnsureAction`
5. Web 角色管理勾选该按钮权限，重新登录桌面端验证

### 第 7 步：联调检查清单

- [ ] 菜单可见，点击能导航到页面
- [ ] Service CRUD / 业务逻辑正常
- [ ] Validator 校验失败能在 UI 显示（Growl）
- [ ] Serilog 日志有记录
- [ ] 需要审计字段时 Service 内填充 CreatedBy/UpdatedBy
- [ ] 版本号、会话等业务横切功能无需额外处理（已 DI 注入）
- [ ] 受控按钮已在「角色权限分配」穿梭框勾选并重新登录验证

---

## 3. 两种页面形态

| 类型 | 场景 | 注册方式 |
|------|------|----------|
| **主导航页** | 侧栏菜单进入的功能页 | `RegisterForNavigation<View, ViewModel>("ViewName")` |
| **弹窗** | 生成结果、改密码等 | 父 ViewModel 打开 Window；ViewModel 仍只注入 Core 接口 |

---

## 4. 数据流

**平台门票（API，无 Refresh Token）**

```
登录 / 选厂 / 切厂 / 改密 / 登出 / 桌面菜单
  → IAuthService / IMenuPermissionService
  → Refit IAuthApi
  → WF.MES.Api
```

**产线业务（直连 DB）**

```
用户操作
  → View (XAML)
  → ViewModel
  → Core 接口 (IXxxService)
  → Infrastructure Service
  → FluentValidation + SqlSugar
  → SQL Server（ConnectionStrings:WfMesDb）
```

ViewModel **不得**绕过接口直接访问 Infrastructure 或数据库；**禁止**产线模块使用 Refit 业务 API。

### 4.1 多语言（i18n）

文案包：`WF.MES.WPF/i18n/{locale}.json`。与 Web `t(key)` 一样：**一个键、一层一种写法**。

#### 键树（必须遵守）

| 前缀 | 用途 | 示例 |
|------|------|------|
| `ui.fields.*` | 字段名（无冒号） | `ui.fields.customer` |
| `ui.actions.*` | 按钮/动词 | `ui.actions.add` |
| `ui.barcode.*` / `ui.app.*` / `ui.mes.*` … | 模块 UI 文案 | `ui.barcode.exportTitle` |
| `ui.*`（扁平） | 壳层/登录相关 UI | `ui.welcomeUser` |
| `err.*` | 业务异常（`BusinessException` / API fallback） | `err.materialRuleNotFound` |
| `val.*` | FluentValidation 消息 | `val.customerNameRequired` |
| `auth.*` / `session.*` / `factory.*` / `common.*` | 与后端 `messageCode` 对齐 | `auth.invalid_credentials` |
| `menu.*` | 菜单（DB `I18nKey`） | `menu.desktop.barcode.print` |

**禁止：** 在 `ui.barcode.*` 里再抛业务异常键；异常一律 `err.*`。校验一律 `val.*`。

#### 标准写法（日常开发只用这些）

登录页 / 主壳层 / 业务模块统一：

| 场景 | 写法 |
|------|------|
| XAML 按钮/标签/复选框 | `infra:Loc.Key="ui.actions.add"` |
| XAML 表单字段标签（带冒号） | `infra:Loc.FieldLabelKey="ui.fields.customer"` |
| DataGrid 列头 | `<infra:LocDataGridTextColumn LocKey="ui.fields.customer" …/>` |
| DataGrid 枚举/状态列 | `<infra:LocDataGridEnumColumn LocKey="…" EnumMap="printStatus" Binding="{Binding PrintStatus}" />` |
| ViewModel 标题/Growl/拼接 | `L("…")` / `TF("ui.barcode.exportedCount", count)` / `EX(ex)` |
| 下拉选项 | `LocalizedOptions.*(L)` + `RefreshLocalizedProperties` |
| 启动早期（无 DI） | `WpfLocalization.T("ui.app.tip")` |

`Resources["Loc"]` 在 **CreateShell 之前**挂好（`App.EnsureUiLocalization`），避免登录页 `Loc.Key` 绑到 Fallback。

壳层与登录页的**静态**文案也必须用 `Loc.Key`（勿再维护 `WelcomeTitle`/`LogoutText` 一类属性再 `RaisePropertyChanged`）；仅动态句（状态栏、`WelcomeUser`、格式化版本号等）留在 ViewModel。HandyControl `Placeholder` 等无法挂 `Loc.Key` 的属性可例外保留 VM 属性。

基础设施文件（实现细节，业务模块勿再拆新类）：

| 文件 | 内容 |
|------|------|
| `Loc.cs` | `Loc` / `WpfLocalization` / `LocalizationBindingSource` |
| `LocalizedViewModelBase.cs` | `L` / `TF` / `EX` |
| `LocColumns.cs` | DataGrid 文本列 / 枚举列 |
| `LocConverters.cs` | 枚举/格式 Converter、`LocalizedOptions` |
| `LocInfoField.cs` | 【高级】信息面板字段行 |

`EnumMap` 见 `LocEnumMaps`。**DTO 只存原始值**，列表文案由 `LocEnumConverter` 翻译。

```xml
xmlns:infra="clr-namespace:WF.MES.WPF.Infrastructure"

<Button infra:Loc.Key="ui.actions.add" Command="{Binding AddCommand}" />
<infra:LocDataGridEnumColumn LocKey="ui.fields.status" EnumMap="enable" Binding="{Binding Enable}" />
```

#### 高级（可选，信息面板）

| 场景 | 写法 |
|------|------|
| 标签+值一行 | `<infra:LocInfoField LabelKey="ui.fields.customer" ValuePath="CustomerName" />` |
| 整句格式化 | `LocFormatConverter` + `MultiBinding`（末位绑 `Loc.LocSource.Revision`） |

#### 异常与校验

- 服务层：`throw new BusinessException("err.xxx", args…)`；UI：`Growl.Error(EX(ex))`
- API：`ApiResponseHelper.EnsureData(result, "err.xxx")`
- FluentValidation：`ILocalizationService` + `val.*`

**禁止：** `IDesktopUiText`、`BindingProxy`、Core 常量写中文 Display、DTO `*Text` 显示字段、`Apply*Text` + `ReloadCollection`、VM 拼接 `DetailXxxLine`。

CSV 导出表头：服务层注入 `ILocalizationService` 取 `ui.barcode.exportCsv.*`，或返回数值由 VM `TF()` 格式化。

---

## 5. 开发红线（拆分后必须遵守）

1. ViewModel 不要引用 Infrastructure（含 static helper、SqlSugar）
2. 不要在 WPF 注册 Service — 统一在 `InfrastructureServiceRegistration.cs`
3. 不要在 Core 自行读取程序集版本 — 使用注入的 `IAppVersion`
4. ViewName 必须与 `System_Menu.Component` 一致
5. 校验规则写在 Infrastructure Validator + Service 入口，不要只在 ViewModel 写

---

## 6. 快速对照：新增「工单扫描」示例

假设新增模组 `Mes.WorkOrderScan`：

| 顺序 | 文件 |
|------|------|
| 1 | `Scripts/Mes_WorkOrderScan.sql` |
| 2 | `Core/Models/Entities/WorkOrder.cs` + Dtos |
| 3 | `Core/Core/Interfaces/IWorkOrderService.cs` |
| 4 | `Infrastructure/Services/Mes/WorkOrderService.cs` |
| 5 | `Infrastructure/Validation/Mes/WorkOrderScanValidator.cs`（如需要） |
| 6 | `Infrastructure/InfrastructureServiceRegistration.cs`（+2 行注册） |
| 7 | `WPF/Modules/Mes/Views/WorkOrderScanView.xaml` |
| 8 | `WPF/Modules/Mes/ViewModels/WorkOrderScanViewModel.cs` |
| 9 | `WPF/App.xaml.cs`（+1 行导航注册） |
| 10 | `Scripts/Mes_WorkOrderScan_Menu.sql` |

---

## 7. 后续扩展（Web API / Windows Service）

同一套 **Core 接口 + Infrastructure Service** 可直接复用：

- 新建 ASP.NET Core 宿主
- 注册相同的 Infrastructure 服务（可改为 `IServiceCollection` 扩展）
- **不必重写业务逻辑**，仅替换 UI 层

---

## 8. 关键文件索引

| 用途 | 路径 |
|------|------|
| DI 注册入口 | `WF.MES.Infrastructure/InfrastructureServiceRegistration.cs` |
| Prism 导航注册 | `WF.MES.WPF/App.xaml.cs` → `RegisterModuleViews` |
| 版本注入 | `WF.MES.WPF/Infrastructure/EntryAssemblyAppVersion.cs` |
| 菜单种子示例 | `database/sql/03_seed_data.sql`（Desktop ClientType=3） |
| 数据库脚本说明 | `WF.MES.WPF/Scripts/README.md` |
| ViewModel 参考 | `WF.MES.WPF/Modules/Barcode/ViewModels/CustomerManageViewModel.cs` |

---

*WF 制造执行系统 · 内部开发文档*
