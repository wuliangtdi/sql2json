# Sql2Json v1.1 — 功能增强计划

## 新增功能清单

- [x] 1. SQL 语法高亮（AvaloniaEdit）
- [x] 2. 导出格式扩展（CSV / Excel）
- [x] 3. 连接字符串模板（选类型自动填充）
- [x] 4. 暗色主题切换
- [x] 5. 任务重试
- [x] 6. 导出完成通知

---

## 1. SQL 语法高亮

**目标：** 用 AvaloniaEdit 替代查询面板的普通 TextBox，支持 SQL 关键字着色、行号显示

**涉及文件：**
- `Sql2Json.App.csproj` — 添加 AvaloniaEdit 包引用
- `Views/QueryView.axaml` — 替换 TextBox 为 TextEditor
- `ViewModels/QueryViewModel.cs` — 绑定方式调整（AvaloniaEdit 用 Document 绑定）
- 新增 `Assets/SqlSyntax.xshd` — SQL 语法高亮定义文件

**实现要点：**
- 使用 `AvaloniaEdit`（Semi.Avalonia.AvaloniaEdit 有主题适配）
- 自定义 SQL 高亮规则：关键字（SELECT/FROM/WHERE/JOIN 等）、字符串、注释、数字
- 保留 AcceptsReturn、等宽字体等现有行为

---

## 2. 导出格式扩展

**目标：** 支持导出为 CSV 和 Excel (.xlsx)，在执行时可选择导出格式

**涉及文件：**
- 新增 `Core/Enums/ExportFormat.cs` — 导出格式枚举（Json / Csv / Excel）
- 新增 `Core/Services/ICsvExportService.cs` + `CsvExportService.cs`
- 新增 `Core/Services/IExcelExportService.cs` + `ExcelExportService.cs`
- `Core/Models/QueryTask.cs` — 添加 ExportFormat 字段
- `Core/Services/TaskExecutorService.cs` — 根据格式调用不同导出服务
- `App/ViewModels/QueryViewModel.cs` — 添加格式选择
- `App/Views/QueryView.axaml` — 添加格式下拉框
- `Sql2Json.Core.csproj` — 添加 ClosedXML 包引用（Excel 导出）

**实现要点：**
- CSV：用 StringBuilder 拼接，首行列名，逗号分隔，值含逗号/换行时加双引号
- Excel：用 ClosedXML 库，每个结果集一个 Sheet
- 文件扩展名根据格式自动切换（.json / .csv / .xlsx）

---

## 3. 连接字符串模板

**目标：** 选择数据库类型后，自动填充对应的连接字符串模板，用户只需替换占位符

**涉及文件：**
- `App/ViewModels/DatabaseConfigViewModel.cs` — 监听类型变更，填充模板
- 新增 `Core/Helpers/ConnectionStringTemplates.cs` — 各数据库类型的模板定义

**模板内容：**
- SQL Server: `Server=localhost;Database=数据库名;User Id=用户名;Password=密码;TrustServerCertificate=True;`
- MySQL: `Server=localhost;Port=3306;Database=数据库名;User=用户名;Password=密码;`
- PostgreSQL: `Host=localhost;Port=5432;Database=数据库名;Username=用户名;Password=密码;`
- Oracle: `Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=服务名)));User Id=用户名;Password=密码;`
- SQLite: `Data Source=数据库文件路径.db;`
- MariaDB: `Server=localhost;Port=3306;Database=数据库名;User=用户名;Password=密码;`

**实现要点：**
- 仅在新增时自动填充（编辑已有配置时不覆盖）
- 连接字符串为空时才填充模板

---

## 4. 暗色主题切换

**目标：** 支持 Light/Dark 主题一键切换，记住用户选择

**涉及文件：**
- `App/App.axaml.cs` — 动态切换 RequestedThemeVariant
- `App/ViewModels/SettingsViewModel.cs` — 添加主题选择属性
- `App/Views/SettingsView.axaml` — 添加主题切换开关
- `Core/Models/AppSettings.cs` — 添加 IsDarkTheme 字段
- `Views/MainWindow.axaml` — 导航栏添加主题切换按钮（快捷入口）

**实现要点：**
- Semi.Avalonia 原生支持 Dark 模式，只需切换 `RequestedThemeVariant`
- 启动时读取设置恢复主题
- 导航栏底部加个月亮/太阳图标快速切换

---

## 5. 任务重试

**目标：** 失败的任务支持一键重新执行，不用重新填写 SQL 和参数

**涉及文件：**
- `App/ViewModels/TaskListViewModel.cs` — 添加 RetryCommand
- `App/Views/TaskListView.axaml` — 操作按钮区添加"重试"按钮

**实现要点：**
- 从失败任务复制 SQL、参数、数据库、文件夹、文件名，创建新 QueryTask
- 新任务加入队列顶部，原失败任务保留（方便对比）
- 只有 Failed 状态的任务显示重试按钮

---

## 6. 导出完成通知

**目标：** 任务完成时发送系统通知，长查询不用一直盯着

**涉及文件：**
- 新增 `App/Services/INotificationService.cs` + `NotificationService.cs`
- `Core/Services/TaskExecutorService.cs` — 任务完成后触发通知
- `App/ViewModels/SettingsViewModel.cs` — 添加通知开关
- `App/Views/SettingsView.axaml` — 添加通知设置
- `Core/Models/AppSettings.cs` — 添加 EnableNotification 字段

**实现要点：**
- Windows：使用系统 Toast 通知（通过 Avalonia 的 Notification API 或 P/Invoke）
- macOS/Linux：使用 Avalonia 内置的窗口通知
- 可配置：开关通知、仅失败时通知、全部通知
- 通知内容：任务名 + 状态 + 耗时

---

## 实施顺序

按依赖关系和复杂度排序：

1. **连接字符串模板**（最简单，独立，5 分钟搞定）
2. **任务重试**（简单，独立）
3. **暗色主题切换**（简单，独立）
4. **SQL 语法高亮**（中等，需要新包）
5. **导出完成通知**（中等，跨平台差异）
6. **导出格式扩展**（最复杂，新增两个导出服务）

---

## 变更记录

| 日期 | 变更内容 |
|------|----------|
| 2026-05-13 | v1.1 增强计划创建 |
