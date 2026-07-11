# WF造业工厂 MES

WF 规范命名的制造业 MES 系统骨架，采用前后端分离架构。

## 项目结构

```
WF.MES/
├── backend/                 # .NET 8 后端（Clean Architecture + DDD 轻量版）
│   ├── WF.MES.Api/
│   ├── WF.MES.Application/
│   ├── WF.MES.Domain/
│   ├── WF.MES.Infrastructure/
│   └── WF.MES.Shared/
├── web/wf-mes-web/          # Vue3 + Vite + Element Plus 管理端
├── mobile/wf-mes-mobile/    # uni-app + Vue3 + uView Plus 移动端
├── database/sql/            # SQL Server 建库建表及种子数据脚本
└── docker/docker-compose.yml
```

## 技术栈

| 层级 | 技术 |
|------|------|
| 后端框架 | .NET 8 / ASP.NET Core Web API |
| ORM | SqlSugar |
| 认证 | JWT + Refresh Token |
| 权限 | RBAC + 数据权限（工厂/车间/产线/班组） |
| 任务 | Quartz |
| 消息 | RabbitMQ |
| 缓存 | Redis |
| 文档 | Swagger |
| 日志 | Serilog |
| 数据库 | SQL Server |
| 前端 | Vue3 + TypeScript + Vite + Element Plus + Pinia + Vxe-Table |
| 移动端 | uni-app + Vue3 + uView Plus |

## 一期模块（账号与权限）

- 用户管理
- 角色管理（含数据权限范围）
- 菜单管理（目录/菜单/按钮）
- 部门 / 班组管理（公司 → 工厂 → 车间 → 产线 → 班组）
- 岗位 / 工序权限

## 数据库初始化

**必须先执行 SQL 脚本**，后端不会自动建表或写入种子数据。

```bash
cd database/sql
sqlcmd -S localhost,1433 -U sa -P "WfMes@123456" -i 00_rebuild_all.sql
```

详见 [`database/README.md`](database/README.md)

## 快速启动

### 1. 启动基础设施

```bash
cd docker
docker compose up -d
```

服务端口：
- SQL Server: `1433`（sa / WfMes@123456）
- Redis: `6379`
- RabbitMQ: `5672`，管理台 `15672`（guest / guest）

### 2. 启动后端

确保已执行数据库脚本后：

```bash
cd backend
dotnet run --project WF.MES.Api
```

- API: http://localhost:5088
- Swagger: http://localhost:5088/swagger

### 3. 启动 Web 管理端

```bash
cd web/wf-mes-web
npm install
npm run dev
```

访问: http://localhost:5173

### 4. 启动移动端 App（可选）

目标平台为 **Android / iOS App**，不是 H5。需安装 [HBuilderX](https://www.dcloud.io/hbuilderx.html)：

```bash
cd mobile/wf-mes-mobile
npm install
npm run dev:app
```

终端保持 `dev:app` 运行后，用 HBuilderX 打开同目录 → **运行到 Android App 基座**。真机调试时 `env/.env.development` 中 API 地址须改为电脑局域网 IP（不能用 `localhost`）。

详见 [mobile/wf-mes-mobile/README.md](mobile/wf-mes-mobile/README.md)。

## 默认账号

| 账号 | 密码 | 说明 |
|------|------|------|
| admin | Admin@123 | 超级管理员 |
| operator | Operator@123 | 车间操作员（含岗位/工序权限） |

## API 规范

- 前缀: `/api/v1`
- 统一响应:

```json
{
  "code": 200,
  "message": "success",
  "data": {},
  "timestamp": 1719900000
}
```

## 后续扩展建议

1. 生产模块：工单、报工、WIP
2. 数据权限过滤器：按角色 DataScope 自动过滤 SqlSugar 查询
3. SignalR 看板推送
4. ERP 集成服务（独立 Integration 层）
5. uni-app 扫码报工、移动巡检页面
