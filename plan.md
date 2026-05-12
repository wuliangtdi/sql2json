# Sql2Json — 实施计划

## 项目概述

一个基于 .NET 10 + EF Core + Avalonia 的桌面工具，用于执行 SQL 查询并将结果导出为 JSON 文件。

## 技术栈

| 组件 | 选型 | 版本要求 |
|------|------|----------|
| 框架 | .NET 10 | 最新稳定版 |
| UI | Avalonia + Semi.Avalonia | 最新稳定版 |
| MVVM | CommunityToolkit.Mvvm | 最新稳定版 |
| ORM/数据访问 | EF Core（动态 SQL 执行 + 本地 SQLite 存储配置/历史） | 最新稳定版 |
| 数据库驱动 | SqlClient / MySqlConnector / Npgsql / Oracle / SQLite | 各自最新稳定版 |
| JSON | System.Text.Json | 内置 |
| 配置持久化 | 本地 SQLite + EF Core (app.db) | — |

## 功能清单

### P0 — 核心功能

| # | 功能 | 说明 |
|---|------|------|
| 1 | SQL 执行 | 输入 SQL 语句，选择目标数据库，执行并获取结果 |
| 2 | 多结果集支持 | 一条 SQL 包含多个 SELECT，返回多个 DataTable |
| 3 | 参数化查询 | 支持 @param 形式的参数，UI 提供参数名/值输入 |
| 4 | JSON 导出 | 将结果序列化为 JSON 文件，支持空结果集导出 |
| 5 | 文件命名 | 执行前必须指定文件名，否则禁止执行 |
| 6 | 输出文件夹 | 可配置多个文件夹，执行时必须选择一个 |
| 7 | 多数据库配置 | 支持 SQL Server / MySQL / PostgreSQL / Oracle / SQLite，可增删改 |
| 8 | 数据库类型选择 | 配置时选择数据库类型，自动加载对应驱动 |
| 9 | 任务队列 | 多次查询放入列表，显示状态（等待中/执行中/已完成/失败） |
| 10 | 配置持久化 | 数据库配置、文件夹配置本地 JSON 存储，启动自动加载 |

### P1 — 体验增强

| # | 功能 | 说明 |
|---|------|------|
| 11 | 连接测试 | 配置数据库时可测试连接是否正常 |
| 12 | 结果预览 | 执行完成后预览前 100 条数据，确认后再导出 |
| 13 | 查询历史 | 保存常用 SQL，可快速选用 |
| 14 | JSON 格式选项 | 缩进（紧凑/美化）、编码（UTF-8 / UTF-8 BOM） |
| 15 | 批量导出 | 任务列表中已完成的任务一键全部导出 |

---

## 架构设计

### 项目结构

```
Sql2Json/
├── Sql2Json.sln
├── src/
│   ├── Sql2Json.Core/                  # 核心业务逻辑（类库）
│   │   ├── Models/                     # 数据模型
│   │   │   ├── DatabaseConfig.cs       # 数据库配置模型
│   │   │   ├── FolderConfig.cs         # 文件夹配置模型
│   │   │   ├── QueryTask.cs            # 查询任务模型（运行时）
│   │   │   ├── QueryParameter.cs       # 查询参数模型
│   │   │   ├── QueryResult.cs          # 查询结果模型
│   │   │   ├── QueryHistory.cs         # 查询历史模型（持久化）
│   │   │   ├── AppSettings.cs          # 应用设置模型
│   │   │   └── JsonExportOptions.cs    # JSON 导出选项
│   │   ├── Data/                       # EF Core 数据层
│   │   │   ├── AppDbContext.cs         # 本地 SQLite DbContext
│   │   │   └── Migrations/            # EF Core 迁移文件
│   │   ├── Services/                   # 服务层
│   │   │   ├── IDatabaseService.cs     # 数据库执行服务接口
│   │   │   ├── DatabaseService.cs      # 数据库执行服务实现
│   │   │   ├── IConfigService.cs       # 配置服务接口
│   │   │   ├── ConfigService.cs        # 配置服务实现
│   │   │   ├── IJsonExportService.cs   # JSON 导出接口
│   │   │   ├── JsonExportService.cs    # JSON 导出实现
│   │   │   ├── ITaskExecutorService.cs # 任务执行器接口
│   │   │   ├── TaskExecutorService.cs  # 任务执行器实现（并发控制）
│   │   │   ├── IQueryHistoryService.cs # 查询历史接口
│   │   │   └── QueryHistoryService.cs  # 查询历史实现
│   │   └── Enums/
│   │       ├── DatabaseType.cs         # 数据库类型枚举
│   │       └── QueryTaskStatus.cs      # 任务状态枚举
│   │
│   └── Sql2Json.App/                   # Avalonia 桌面应用
│       ├── App.axaml                   # 应用入口
│       ├── Program.cs
│       ├── ViewModels/                 # ViewModel 层
│       │   ├── MainWindowViewModel.cs  # 主窗口 VM
│       │   ├── QueryViewModel.cs       # 查询面板 VM
│       │   ├── TaskListViewModel.cs    # 任务列表 VM
│       │   ├── DatabaseConfigViewModel.cs  # 数据库配置 VM
│       │   ├── FolderConfigViewModel.cs    # 文件夹配置 VM
│       │   └── SettingsViewModel.cs    # 设置 VM
│       ├── Views/                      # View 层
│       │   ├── MainWindow.axaml        # 主窗口
│       │   ├── QueryView.axaml         # 查询面板
│       │   ├── TaskListView.axaml      # 任务列表
│       │   ├── DatabaseConfigView.axaml    # 数据库配置
│       │   ├── FolderConfigView.axaml      # 文件夹配置
│       │   ├── ResultPreviewView.axaml     # 结果预览
│       │   └── SettingsView.axaml      # 设置页
│       ├── Converters/                 # 值转换器
│       │   └── TaskStatusConverter.cs
│       └── Services/                   # UI 层服务
│           └── DialogService.cs        # 对话框服务
```

### 分层职责

```
┌─────────────────────────────────────────────┐
│  Views (AXAML)                              │  纯 UI 展示，无业务逻辑
├─────────────────────────────────────────────┤
│  ViewModels (CommunityToolkit.Mvvm)         │  UI 状态管理、命令绑定、输入验证
├─────────────────────────────────────────────┤
│  Core Services                              │  业务逻辑：数据库执行、JSON 导出、配置管理
├─────────────────────────────────────────────┤
│  Data Access (ADO.NET via DbConnection)     │  数据库连接、SQL 执行、结果读取
└─────────────────────────────────────────────┘
```

### 数据库驱动策略

根据用户选择的数据库类型，通过 EF Core 的 `DbContext` 配置对应的 Provider：

| 数据库类型 | NuGet 包 | EF Core Provider |
|-----------|----------|------------------|
| SQL Server | Microsoft.EntityFrameworkCore.SqlServer | UseSqlServer() |
| MySQL | Pomelo.EntityFrameworkCore.MySql | UseMySql() |
| PostgreSQL | Npgsql.EntityFrameworkCore.PostgreSQL | UseNpgsql() |
| Oracle | Oracle.EntityFrameworkCore | UseOracle() |
| SQLite | Microsoft.EntityFrameworkCore.Sqlite | UseSqlite() |
| MariaDB | Pomelo.EntityFrameworkCore.MySql (兼容) | UseMySql() |

#### EF Core 使用方式

1. **动态 SQL 执行**：通过 `DbContext.Database.GetDbConnection()` 获取底层连接，使用 `DbCommand` + `DbDataReader` 执行用户输入的任意 SQL（因为返回结构在编译期未知）
2. **本地数据存储**：使用独立的 `AppDbContext` + SQLite 存储配置信息、查询历史等结构化数据，享受 EF Core 的迁移和 LINQ 能力
3. **连接生命周期**：每个查询任务创建独立的 `DbContext` 实例，任务完成后释放，支持并发执行

### 任务队列设计

```
用户提交查询 → 创建 QueryTask（状态：等待中）→ 加入任务列表
                                                    ↓
                                          立即启动独立 Task 执行
                                                    ↓
                                          状态变更：执行中
                                                    ↓
                                          执行 SQL，获取结果（独立 DbContext）
                                                    ↓
                                    成功 → 状态：已完成（可预览/导出）
                                    失败 → 状态：失败（显示错误信息）
```

- 每个任务启动独立的 `Task`，创建独立的 `DbContext` / `DbConnection`
- 使用 `SemaphoreSlim` 控制最大并发数（默认 5，可配置），防止同时打开过多连接
- 多个任务可同时执行，互不阻塞
- 状态变更通过 `INotifyPropertyChanged` + `Dispatcher` 实时反映到 UI
- 取消支持：每个任务持有 `CancellationTokenSource`，可单独取消

### 配置持久化方案

使用本地 SQLite 数据库（通过 EF Core 管理），存储路径：`{AppData}/Sql2Json/app.db`

#### AppDbContext 实体：

- `DatabaseConfig` — 数据库连接配置
- `FolderConfig` — 输出文件夹配置
- `QueryHistory` — 查询历史记录
- `AppSettings` — 应用设置（JSON 格式选项、最大并发数等）

#### 优势：
- 结构化存储，支持复杂查询
- EF Core Migration 管理 schema 变更
- 事务支持，数据一致性有保障
- 比手动读写 JSON 文件更健壮

---

## UI 布局设计

### 主窗口布局

```
┌──────────────────────────────────────────────────────────────────┐
│  [数据库配置]  [文件夹配置]  [设置]                    Sql2Json   │
├────────────────────────────────┬─────────────────────────────────┤
│  查询面板                      │  任务列表                        │
│                                │                                 │
│  数据库: [▼ 选择数据库]        │  ┌─────────────────────────────┐│
│  文件夹: [▼ 选择输出文件夹]    │  │ ✓ 用户列表.json   已完成    ││
│  文件名: [____________.json]   │  │ ⟳ 订单数据.json   执行中    ││
│                                │  │ ○ 商品信息.json   等待中    ││
│  ┌────────────────────────┐    │  │                             ││
│  │ SELECT * FROM Users    │    │  └─────────────────────────────┘│
│  │ WHERE Id = @id         │    │                                 │
│  │                        │    │  [批量导出已完成]  [清空列表]     │
│  └────────────────────────┘    │                                 │
│                                ├─────────────────────────────────┤
│  参数:                         │  结果预览                        │
│  ┌──────┬──────────┐           │                                 │
│  │ @id  │ 123      │           │  ┌─────────────────────────────┐│
│  └──────┴──────────┘           │  │ [表格预览前100条]            ││
│  [+ 添加参数]                  │  │  Id | Name | Email          ││
│                                │  │  1  | 张三 | z@test.com     ││
│  历史: [▼ 选择历史查询]        │  │  2  | 李四 | l@test.com     ││
│                                │  └─────────────────────────────┘│
│  [执行并加入队列]              │  [导出此结果]                    │
└────────────────────────────────┴─────────────────────────────────┘
```

---

## 实施步骤

### Phase 1：项目搭建（基础骨架）

1. 创建解决方案和项目结构
2. 添加 NuGet 包引用
3. 配置 Avalonia + Semi.Avalonia 主题
4. 搭建主窗口基本布局框架

### Phase 2：核心模型与服务

5. 定义所有数据模型（Models + Enums）
6. 实现 ConfigService（配置读写）
7. 实现 DatabaseService（连接创建、SQL 执行、多结果集）
8. 实现 JsonExportService（序列化、格式选项、文件写入）
9. 实现 QueryHistoryService（历史记录 CRUD）

### Phase 3：ViewModel 层

10. 实现 MainWindowViewModel（导航、全局状态）
11. 实现 QueryViewModel（查询输入、参数管理、验证）
12. 实现 TaskListViewModel（任务队列、状态管理、后台执行）
13. 实现 DatabaseConfigViewModel（数据库增删改、连接测试）
14. 实现 FolderConfigViewModel（文件夹增删改）
15. 实现 SettingsViewModel（JSON 格式选项）

### Phase 4：View 层

16. 实现 QueryView（SQL 输入、参数表、历史选择）
17. 实现 TaskListView（任务列表、状态标识）
18. 实现 ResultPreviewView（数据表格预览）
19. 实现 DatabaseConfigView（数据库配置对话框）
20. 实现 FolderConfigView（文件夹配置对话框）
21. 实现 SettingsView（设置页面）
22. 实现 MainWindow（整体布局组装）

### Phase 5：集成与打磨

23. 接入 DI 容器，注册所有服务
24. 实现批量导出功能
25. 错误处理与用户提示
26. UI 细节打磨（加载动画、空状态提示）
27. 整体测试与修复

---

## 关键设计决策

### 为什么用 EF Core？

1. **动态 SQL 执行**：通过 `DbContext.Database.GetDbConnection()` 获取连接，用 `DbDataReader` 读取未知结构的结果集。虽然动态 SQL 无法利用 EF Core 的实体映射，但 EF Core 统一管理连接创建、Provider 切换、连接字符串配置
2. **本地数据管理**：配置信息、查询历史用 SQLite + EF Core 存储，享受 Migration、LINQ、强类型查询的便利
3. **Provider 统一切换**：通过 EF Core 的 Provider 体系，一套代码支持多种数据库，切换数据库只需更换 `UseXxx()` 调用

### 为什么用 SemaphoreSlim 控制并发？

任务队列支持多任务同时执行，但需要限制最大并发数：
- 避免同时打开过多数据库连接耗尽连接池
- 避免目标数据库压力过大
- 每个任务独立 DbContext，无共享状态，天然线程安全

### 多结果集处理策略

一条 SQL 可能包含多个 SELECT 语句，使用 `DbDataReader.NextResult()` 遍历所有结果集。导出时：
- 单结果集 → 直接导出为 JSON 数组
- 多结果集 → 导出为 JSON 对象，key 为 `result_0`, `result_1`, ...

---

## 风险与注意事项

1. **连接字符串安全** — 配置文件中的连接字符串包含敏感信息，后续可考虑加密存储
2. **大结果集内存** — 如果查询返回百万行，内存可能不足。初版限制预览 100 条，导出时流式写入
3. **SQL 注入** — 参数化查询已规避，但需确保参数绑定正确实现
4. **跨平台路径** — Avalonia 支持跨平台，文件路径处理需注意 Windows/Linux 差异
5. **数据库驱动兼容性** — Oracle 驱动在某些平台可能有问题，需要测试

---

## 变更记录

| 日期 | 变更内容 | 原因 |
|------|----------|------|
| 2026-05-12 | 初始计划创建 | 项目启动 |
