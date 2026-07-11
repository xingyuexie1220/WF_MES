# WF.MES Backend

ASP.NET Core 8 Web API，六层架构：`Api` / `Application` / `Domain` / `Infrastructure` / `Contracts` / `Shared`。

## 本地运行

```bash
cd backend
dotnet build WF.MES.Api/WF.MES.Api.csproj
dotnet run --project WF.MES.Api
```

- Swagger：`http://localhost:5xxx/swagger`
- 健康检查：`GET /health`（SQL Server、Redis、MQTT）
- SignalR：`/hubs/notice`（JWT，`wf:factory_code` 声明自动加入工厂分组）

## 配置说明

### Redis（会话 + 业务缓存）

`appsettings.json` → `Redis`：

| 键 | 说明 |
|----|------|
| `ConnectionString` | StackExchange.Redis 连接串 |
| `DefaultDatabase` | 默认 DB 索引 |

`Cache` 节控制业务缓存 TTL（分钟）：

| 键 | 默认 | 用途 |
|----|------|------|
| `DictTtlMinutes` | 30 | 数据字典 `GetDataByType` |
| `MenuTtlMinutes` | 15 | 登录后菜单/路由 |
| `DashboardTtlMinutes` | 1 | 大屏 KPI scaffold |

Redis Key 约定：

- 会话：`wf:session:{factoryId}:{userId}:{clientType}`
- 会话元数据：`wf:session:meta:{sessionId}`（IP / UA）
- Refresh 黑名单：`wf:rt:blacklist:{token}`
- 业务缓存：`wf:cache:{factoryId}:{category}:{key}`
- 设备快照：`wf:device:{factoryCode}:{deviceId}:snapshot`

**降级策略**：Redis 不可用时，`SessionValidationMiddleware` 会放行（`IsActiveSessionAsync` 返回 `true`）。生产环境应监控 `/health` 的 `redis` 检查，避免 silent bypass。

### EMQX / MQTT（设备消息）

`Mqtt` 节（默认 `Enabled: false`，本地无 Broker 时可保持关闭）：

```json
"Mqtt": {
  "Enabled": true,
  "Host": "127.0.0.1",
  "Port": 1883,
  "ClientId": "wf-mes-api",
  "SubscribeTopics": [
    "wf/+/equipment/+/telemetry",
    "wf/+/equipment/+/alarm",
    "wf/+/equipment/+/status"
  ]
}
```

Topic 约定：

```
wf/{factoryCode}/equipment/{deviceId}/telemetry
wf/{factoryCode}/equipment/{deviceId}/alarm
wf/{factoryCode}/equipment/{deviceId}/status
```

EMQX ACL 建议：

- **设备账号**：仅 `publish` → `wf/{factoryCode}/equipment/{deviceId}/#`
- **API 服务账号**：仅 `subscribe` → `wf/+/equipment/+/#`

消息链路：EMQX → `MqttHostedService` → `EquipmentTelemetryHandler` → Redis 设备快照 + SignalR `DeviceTelemetryReceived`。

测试发布（mosquitto_pub / EMQX Dashboard）：

```bash
mosquitto_pub -h 127.0.0.1 -t "wf/DEV/equipment/PLC-01/telemetry" -m '{"output":120,"cycleTime":1.2}'
```

### RabbitMQ

`RabbitMqPublisher` 已注册 DI，供内部异步任务（打印队列、报表等）；**设备侧走 MQTT**，与 RabbitMQ 职责分离。短期未使用时保持配置即可。

## 已实现能力摘要

- JWT + RefreshToken + 同端互踢（Redis 会话）
- Logout / Refresh 黑名单
- FluentValidation（Login、条码客户保存等）
- Quartz 日志清理 Job
- 设备遥测实时推送（MQTT + SignalR）
