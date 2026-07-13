# WF.MES Web 管理端

Vue 3 + TypeScript + Vite + Pinia + Element Plus + ECharts + VxeTable。

## 目录结构

```
src/
├── api/              # 按业务域 REST 封装
│   ├── auth/         # 登录、工厂切换、路由
│   ├── system/       # 系统管理 CRUD
│   ├── dashboard/    # 看板 / 大屏
│   ├── barcode/      # 条码（客户等）
│   ├── production/   # 生产工单（scaffold）
│   └── equipment/    # 设备测试（scaffold）
├── assets/           # 静态资源；styles/_tokens.scss 设计变量
├── components/       # 全局组件（page/、barcode/）
├── composables/      # useDict、useLargeList 等
├── config/           # 菜单 fallback、titleKey 映射
├── i18n/             # vue-i18n 文案（locales/*.ts 为源）
├── layout/           # WfLayout 壳（Header/Sidebar/Tabs/Content）
├── router/
│   ├── guards.ts     # 登录鉴权、权限、nprogress
│   └── routes/       # auth | system | master-data | report | bigscreen | barcode
├── stores/
│   ├── auth/         # 用户、Token、权限
│   ├── layout/       # 菜单、页签、全局 loading/theme
│   ├── notice/       # 站内消息
│   ├── mes/          # 工单缓存（scaffold）
│   ├── barcode/      # 条码客户缓存
│   └── dashboard/    # 大屏/报表 overview
├── types/            # 与 C# DTO 对齐（按域分子目录）
├── utils/            # request/http、token、format、download、print、exportExcel
└── views/
    ├── auth/         # 登录、改密
    ├── system/       # 11 个系统管理页
    ├── dashboard/    # 首页看板
    ├── bigscreen/    # 数据大屏
    ├── barcode/      # 条码业务页
    └── common/developing/  # master-data、report 占位页
```

## 命名约定

| 类别 | 约定 | 示例 |
|------|------|------|
| 共享 Vue | PascalCase + `Wf` 前缀 | `layout/WfHeader.vue`、`components/page/WfTable.vue` |
| 页面 | `views/{域}/{kebab}/index.vue` | `views/system/user/index.vue` |
| API / types / routes | kebab-case 文件 | `api/system/user.ts`、`types/system/role.ts` |
| composables / config | camelCase | `useDict.ts`、`menuTitleKeyMap.ts` |
| 样式 partial | `_kebab.scss` | `styles/page/_list-panel.scss` |
| 导入 | **域路径直达** | `@/types/system/user`、`@/stores/auth/user` |

**禁止：** `@/types`、`@/stores` 根 barrel；已移除的 `@/types/api` shim。

**列表页：** 统一 `wf-list-panel` + `WfPage` / `WfPageBody` / `WfPagePager`（及按需 `WfTable` / `WfVxeTable`）。不要再引入已删除的 `WfPageSearch` / `WfPageToolbar`。

## 开发

```bash
npm install
npm run dev
```

- 本地 API 代理：`vite.config.ts` 将 `/api`、`/hubs` 转发到 `http://localhost:5088`
- 环境变量：`.env.development` / `.env.production`（参考 `.env.example`）

## 构建

```bash
npm run build
npm run preview
```

## 代码格式

```bash
npm run format
npm run format:check
```

## Nginx 部署示例

见 [`deploy/nginx.conf.example`](deploy/nginx.conf.example)。

要点：

- 静态资源：`root` 指向 `dist`，SPA 使用 `try_files $uri /index.html`
- API 反代：`/api/` → 后端；`/hubs/` → SignalR（WebSocket）
- 开启 `gzip`，带 hash 的静态资源可长期缓存

## 类型与 API 约定

- 类型定义在 `src/types/{domain}/`，与 C# DTO 字段 camelCase 对齐
- Store 统一 `@/stores/auth/user`、`@/stores/layout/menu` 等域路径
- 统一使用 `http.get/post/put/delete`（`src/utils/request/http.ts`）
- 响应体由拦截器解包 `ApiResult.data`，调用方直接获得业务数据

## i18n 说明

- Web UI 文案：**独立维护** `src/i18n/locales/{zh-CN,en,zh-TW}.ts`
- API `messageCode` 使用 **snake_case** 键（与 [i18n/api-codes/](../../i18n/api-codes/) 一致），经 `resolveApiMessage` 翻译
- 新增 `WfMessageCodes` 时，在 locales 中**手工**补同名键；可用 `node i18n/scripts/validate-api-codes.mjs` 校验

## Scaffold 说明

以下模块已有 API/Store/路由骨架，页面待迭代（**保留，勿当死代码删**）：

| 模块 | 路由 | 占位 |
|------|------|------|
| master-data | `/master-data/*` | `common/developing` |
| report | `/report/*` | `common/developing` |
| production | 无 | `stores/mes` + `api/production` |
| equipment | 无 | `api/equipment` |
