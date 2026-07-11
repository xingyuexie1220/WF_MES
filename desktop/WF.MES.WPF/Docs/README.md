# WF MES 交付文档总览

> 适用版本：**v1.0.1.2**  
> 更新日期：2026-07-10

## 文档清单

| 编号 | 文件 | 读者 | 用途 |
|------|------|------|------|
| 01 | [01_部署安装手册_内部IT.md](./01_部署安装手册_内部IT.md) | 内部 IT | 新环境安装、配置、发布 |
| 02 | [02_用户操作手册_产线用户.md](./02_用户操作手册_产线用户.md) | 产线用户 | 日常条码业务操作 |
| 03 | [03_运维与故障排查_内部IT.md](./03_运维与故障排查_内部IT.md) | 内部 IT | 日志、备份、计划任务、排错 |
| 04 | [04_功能清单与验收用例.md](./04_功能清单与验收用例.md) | IT / 业务 | 初版范围与 UAT |
| — | [WF_MES_新模组开发流程.md](./WF_MES_新模组开发流程.md) | 开发 | 二次开发规范 |
| — | [ReleaseNotes_v1.0.1.2.md](./ReleaseNotes_v1.0.1.2.md) | 全员 | 版本说明 |

配置模板：`../appsettings.example.json`（部署时复制为 `appsettings.json`）

---

## 导出 PDF

Markdown 可用以下方式转为 PDF（任选其一）：

### 方式 A：Pandoc（推荐，适合批量）

在仓库根目录 PowerShell 执行（需已安装 [Pandoc](https://pandoc.org/) 与 LaTeX 或 wkhtmltopdf）：

```powershell
cd WF.MES.WPF\Docs
$files = @(
  "01_部署安装手册_内部IT.md",
  "02_用户操作手册_产线用户.md",
  "03_运维与故障排查_内部IT.md",
  "04_功能清单与验收用例.md"
)
foreach ($f in $files) {
  $pdf = [System.IO.Path]::ChangeExtension($f, ".pdf")
  pandoc $f -o $pdf --pdf-engine=xelatex -V mainfont="Microsoft YaHei" -V geometry:margin=2.5cm
  Write-Host "已生成 $pdf"
}
```

若中文乱码，可改用 VS Code 插件「Markdown PDF」单文件导出。

### 方式 B：VS Code / Cursor

1. 安装扩展 **Markdown PDF**
2. 打开 `.md` 文件 → 命令面板 → `Markdown PDF: Export (pdf)`

### 方式 C：Word 中转

1. 用 Word / Typora 打开 Markdown  
2. 另存为 PDF  

---

## 建议分发方式

| 对象 | 建议文档 |
|------|----------|
| 工位操作员 | 02 用户操作手册（PDF） |
| 条码文员 / QA | 02 用户操作手册（PDF） |
| 信息部 | 01 + 03 + 04（PDF） |
| 项目经理 / 验收 | 04 功能清单与验收用例（PDF） |

---

*WF 制造执行系统 · 内部交付文档*
