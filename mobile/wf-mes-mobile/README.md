# WF MES 移动端（uni-app App）

Android / iOS 原生 App：**uni-app + Vue3 + Pinia + uView Plus + vue-i18n**。

## 功能概览

| 能力 | 说明 |
|------|------|
| 多语言 | 简体中文 / 繁體中文 / English，UI 包见 `src/i18n/locales/`（独立维护） |
| 多工厂 | 登录选厂、我的页切换工厂（`switch-factory` API） |
| Tab 布局 | 工作台 / 扫码 / 我的（借鉴示例项目布局） |
| 鉴权 | JWT 双 Token、401 无感刷新、同端互踢跳转登录 |

## 目录结构

```
src/
├── api/auth.ts           # 认证 API
├── components/           # WfLocalePicker、WfFactoryPicker
├── pages/                # login、home、scan、mine + 业务子页
├── stores/               # user、locale（Pinia）
├── types/auth.ts         # 与后端 DTO 对齐
├── utils/auth.ts         # Token / 工厂 / 语言持久化
├── utils/request.ts      # uni.request 封装 + Refresh
└── i18n/
    ├── index.ts            # vue-i18n
    └── locales/*.json      # 手机 UI 文案包（API messageCode 见 i18n/api-codes）
```

## 安装与运行

```bash
cd mobile/wf-mes-mobile
npm install
npm run dev:app   # 配合 HBuilderX 运行到 Android 基座
```

真机调试请将 `env/.env.development` 中 API 改为局域网 IP：

```env
VITE_API_BASE_URL=http://192.168.1.100:5088/api/v1
```

## 多工厂流程

1. 登录时若账号绑定多个工厂 → 底部弹窗选厂（`select-factory`）
2. 已登录用户在 **我的 → 切换工厂** 调用 `switch-factory`，无需重新输入密码
3. 请求头自动携带 `X-Factory-Id` 与 `Accept-Language`

## 多语言

- 登录页右上角、我的页均可切换语言
- 语言存储键：`wf_locale`（与 Web 一致）
- 切换后 Tab 文案与页面标题同步更新

## 默认账号

与 Web 相同：`operator/Operator@123`（`clientType=2` 移动端）
