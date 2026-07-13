# WF.MES 国际化架构

## 原则

| 层级 | 维护方式 | 说明 |
|------|----------|------|
| **UI 文案** | **三端各一套** | 桌面 / Web / 手机各自维护本端界面文案，互不覆盖 |
| **API 错误** | **协议统一** | 后端只返回 `messageCode`（见 `WfMessageCodes`），各端用 `t(messageCode)` 翻译 |
| **菜单标题** | **键名统一** | 数据库 `System_Menu.I18nKey` 三端均需能 `t(i18nKey, fallbackTitle)` |

**不共用 UI 翻译文件。** 统一的是 **键名协议**（`messageCode` / `I18nKey`），不是共用一个 JSON/TS 包。

---

## 目录结构

```
i18n/
├── README.md                 # 本文件（架构规范）
├── api-codes/                # API messageCode 权威参考（三端须镜像同名键）
│   ├── zh-CN.json
│   ├── zh-TW.json
│   └── en.json
└── scripts/
    ├── sync-api-codes.mjs    # 将 api-codes 合并进桌面/手机 UI 包
    ├── validate-api-codes.mjs
    └── sync-desktop.ps1      # 转发至 sync-api-codes.mjs

desktop/WF.MES.WPF/i18n/      # 桌面 UI 包（构建复制到输出 i18n/）
web/wf-mes-web/src/i18n/locales/   # Web UI 包（*.ts）
mobile/wf-mes-mobile/src/i18n/locales/  # 手机 UI 包（*.json）
```

---

## API 响应协议（方案 B）

成功与业务失败时，`ApiResult` 形态：

```json
{
  "success": false,
  "code": 401,
  "message": "",
  "messageCode": "auth.invalid_credentials",
  "data": null
}
```

- `message`：**空字符串**（有 `messageCode` 时）
- `messageCode`：点分键名，与 `WfMessageCodes` 常量一致
- 各端拦截器：`message = t(messageCode) ?? messageCode ?? t('common.requestFailed')`

后端定义：`backend/WF.MES.Shared/Constants/WfMessageCodes.cs`

权威文案参考：`i18n/api-codes/*.json`（新增 code 时先改此处，再同步三端）

---

## 菜单 I18nKey

`System_Menu.I18nKey` 示例：`menu.desktop.barcode`、`menu.system`

- 三端侧栏/菜单渲染：`t(menu.I18nKey, menu.Title)`
- 各端 UI 包须包含本端会用到的 `menu.*` 键（桌面含 `menu.desktop.*`，手机含 `menu.mobile.*`，Web 含后台路由相关键）

---

## 各端接入

### 桌面（WPF）

- UI 包：`desktop/WF.MES.WPF/i18n/{locale}.json`
- **键树**：`ui.*`（界面）/ `err.*`（业务异常）/ `val.*`（校验）；API 码仍为 `auth.*` / `common.*` 等。桌面包**不含** `mobile.*`（手机端独立维护）。
- 运行时：`JsonLocalizationService` 加载输出目录 `i18n/{locale}.json`
- API 解析：`ApiMessageResolver` / `ApiErrorHelper`
- **WPF 接入规范**（公开 API：`infra:Loc.Key` / `LocDataGrid*` / `L|TF|EX`；实现收拢为 `Loc.cs`、`LocColumns.cs`、`LocConverters.cs`、`LocalizedViewModelBase.cs`，高级：`LocInfoField`）：见 [WF_MES_新模组开发流程.md §4.1](../desktop/WF.MES.WPF/Docs/WF_MES_新模组开发流程.md)

### Web（Vue）

- UI 包：`web/wf-mes-web/src/i18n/locales/*.ts`（独立维护，手工补 API messageCode）
- API 解析：`src/utils/i18n/resolveApiMessage.ts`

### 手机（uni-app）

- UI 包：`mobile/wf-mes-mobile/src/i18n/locales/*.json`
- API 解析：`src/utils/apiMessage.ts`

---

## 新增 messageCode 流程

1. 在 `WfMessageCodes.cs` 增加常量（如 `auth.xxx`）
2. 在 `i18n/api-codes/` 三语言补全文案
3. 运行合并脚本（桌面 + 手机）：
   ```bash
   node i18n/scripts/sync-api-codes.mjs
   ```
4. 在 Web `locales/*.ts` **手工**补同名键（结构与 api-codes 一致）
5. 校验：
   ```bash
   node i18n/scripts/validate-api-codes.mjs
   ```

---

## 脚本说明

| 脚本 | 用途 |
|------|------|
| `sync-api-codes.mjs` | 深度合并 `api-codes` → 桌面/手机 UI 包（不删已有 UI 键） |
| `validate-api-codes.mjs` | 检查三端是否包含全部 `WfMessageCodes` 键 |
| `sync-desktop.ps1` | 转发至 `sync-api-codes.mjs` |

---

## 语言与请求头

- 支持：`zh-CN`、`zh-TW`、`en`
- 客户端请求携带 `Accept-Language`，与本地 UI 语言一致
- 语言持久化键：`wf_locale`（Web / 手机）；桌面：`%LocalAppData%/WF.MES/wf_locale.txt`
