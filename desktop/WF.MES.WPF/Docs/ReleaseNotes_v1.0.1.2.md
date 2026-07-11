# Release Notes — v1.0.1.2

> 发布日期：2026-07-10 · 类型：初版功能发布

---

## 概述

WF MES 桌面客户端初版，完成条码全链路（规则 → QA → 生成/打印 → 明细/补打）及系统权限管理。架构拆分为 Core / Infrastructure / WPF 三层。

---

## 新功能

### 条码管理

- 客户维护
- 料号条码规则（多段组成：字面量 / 日期 / 流水号）
- 条码资料审核（上传图纸与实物图、QA 确认/驳回）
- 条码批量生成（事务 + 流水号锁定）
- BarTender 标签打印（`Labels/{料号}.btw`）
- 条码明细查询与 CSV 导出
- 条码补打（已打印 → 已补打，单次）

### 系统管理

- 用户 / 角色 CRUD
- 角色权限分配（模组、菜单、受控按钮）
- 默认角色：ADMIN、OPERATOR

### 平台能力

- 单实例运行、数据库会话（同机重登 / 异机踢人 / 心跳超时）
- 菜单与受控操作权限
- Serilog 文件日志
- 在线更新（WF.MES.Updater + version.json）

---

## 技术说明

- .NET 8 WPF，目标平台 **x86**
- SQL Server + SqlSugar
- Prism + HandyControl + FluentValidation

---

## 数据库

全新安装顺序：

1. `WF_MES_Init.sql`
2. `Barcode_Schema.sql`

---

## 配置

部署时复制 `appsettings.example.json` → `appsettings.json`，配置数据库连接与 PasswordKey。

---

## 已知问题 / 限制

- MES、物料四个菜单为占位页
- 会话失效后退出整程序
- 密码哈希为 MD5（规划后续升级）

---

## 升级说明

自本版起，通过内网 `version.json` 推送 zip 包；保留各工位 `appsettings.json` 不被覆盖。

---

*WF 制造执行系统*
