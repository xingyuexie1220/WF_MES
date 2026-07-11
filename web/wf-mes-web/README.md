# WF.MES Web 管理端

Vue 3 + TypeScript + Vite + Pinia + Element Plus + ECharts + VxeTable。

## 目录结构

```
src/
├── api/              # 按业务域 REST 封装
│   ├── auth/         # 登录、工厂切换、路由
│   ├── system/       # 系统管理 CRUD
│   ├── dashboard/    # 看板 / 大屏
│   ├── barcode/      # 条码（客户、规则、打印）
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
│   ├── barcode/      # 条码客户/规则缓存
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
- **禁止** 使用已移除的 `@/types/api` shim，请直接 `@/types/system/user` 等域路径
- Store 统一 `@/stores/auth/user`、`@/stores/layout/menu` 等，勿用根目录 shim
- 统一使用 `http.get/post/put/delete`（`src/utils/request/http.ts`）
- 响应体由拦截器解包 `ApiResult.data`，调用方直接获得业务数据

## i18n 说明

- Web 端文案源文件：`src/i18n/locales/{zh-CN,en,zh-TW}.ts`
- 根目录 `i18n/messages/*.json` 供后端/移动端同步；**不要**对 Web 直接跑 `sync-web.mjs` 覆盖 locales（会丢失 `route.*`、`system.*` 等键）
- 新增键请直接编辑 `src/i18n/locales/*.ts`，或改造 sync 脚本为 merge 模式后再使用

## Scaffold 说明

以下模块已有 API/Store/路由骨架，页面待迭代：

| 模块 | 路由 | 占位 |
|------|------|------|
| master-data | `/master-data/*` | `common/developing` |
| report | `/report/*` | `common/developing` |
| production | 无 | `stores/mes` + `api/production` |
| equipment | 无 | `api/equipment` |
