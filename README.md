# Sql2Json

SQL 查询结果导出为 JSON 文件的跨平台桌面工具。

## 功能特性

- **多数据库支持** — SQL Server、MySQL、PostgreSQL、Oracle、SQLite、MariaDB
- **参数化查询** — 支持 `@param` 形式的参数，避免 SQL 注入
- **多结果集** — 一条 SQL 包含多个 SELECT，自动拆分导出
- **并发执行** — 多个查询任务同时执行，互不阻塞
- **自动导出** — 查询完成后自动生成 JSON 文件到指定目录
- **任务队列** — 实时显示任务状态（等待/执行中/完成/失败）
- **结果预览** — 执行完成后可预览 JSON 内容
- **连接测试** — 配置数据库时可测试连接是否正常
- **查询历史** — 保存常用 SQL，快速复用
- **JSON 格式选项** — 缩进/紧凑、UTF-8 BOM、驼峰命名
- **批量导出** — 已完成任务一键全部导出
- **跨平台** — Windows、macOS、Linux

## 技术栈

| 组件 | 选型 |
|------|------|
| 框架 | .NET 10 |
| UI | Avalonia 12 + Semi.Avalonia |
| MVVM | CommunityToolkit.Mvvm |
| 数据存储 | EF Core + SQLite（本地配置） |
| JSON | System.Text.Json |

## 快速开始

### 环境要求

- .NET 10 SDK

### 开发运行

```bash
git clone https://github.com/wuliangtdi/Sql2Json.git
cd Sql2Json
dotnet run --project src/Sql2Json.App
```

### 发布

```bash
# Windows
dotnet publish src/Sql2Json.App -c Release -r win-x64 --self-contained true -o publish/win-x64

# macOS (Apple Silicon)
dotnet publish src/Sql2Json.App -c Release -r osx-arm64 --self-contained true -o publish/osx-arm64

# Linux
dotnet publish src/Sql2Json.App -c Release -r linux-x64 --self-contained true -o publish/linux-x64
```

## 使用说明

1. **配置数据库** — 左侧导航点击「数据库」，新增连接配置，选择数据库类型并填写连接字符串
2. **配置文件夹** — 左侧导航点击「文件夹」，添加 JSON 输出目录
3. **执行查询** — 回到查询页，选择数据库和输出文件夹，输入文件名和 SQL，点击执行
4. **查看结果** — 右侧任务队列实时显示执行状态，完成后可预览 JSON

## 项目结构

```
src/
├── Sql2Json.Core/          # 核心业务逻辑
│   ├── Data/               # EF Core DbContext
│   ├── Enums/              # 枚举定义
│   ├── Models/             # 数据模型
│   └── Services/           # 服务层（数据库执行、JSON导出、配置管理）
└── Sql2Json.App/           # Avalonia 桌面应用
    ├── Converters/         # 值转换器
    ├── ViewModels/         # MVVM ViewModel
    └── Views/              # AXAML 视图
```

## 连接字符串示例

**SQL Server:**
```
Server=localhost;Database=mydb;User Id=sa;Password=123456;TrustServerCertificate=True;
```

**MySQL:**
```
Server=localhost;Port=3306;Database=mydb;User=root;Password=123456;
```

**PostgreSQL:**
```
Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=123456;
```

**SQLite（指你要查询的 SQLite 数据库文件，非应用自身数据）:**
```
Data Source=/path/to/your-database.db;
```

> 注：应用自身的配置数据存储在程序目录下的 `data/app.db`，无需手动配置。

## License

MIT
