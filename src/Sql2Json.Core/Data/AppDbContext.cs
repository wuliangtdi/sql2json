using Microsoft.EntityFrameworkCore;
using Sql2Json.Core.Models;

namespace Sql2Json.Core.Data;

/// <summary>
/// 本地 SQLite 数据库上下文，用于持久化应用配置和查询历史
/// 存储路径：{应用程序目录}/data/app.db
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// 数据库连接配置表
    /// </summary>
    public DbSet<DatabaseConfig> DatabaseConfigs => Set<DatabaseConfig>();

    /// <summary>
    /// 输出文件夹配置表
    /// </summary>
    public DbSet<FolderConfig> FolderConfigs => Set<FolderConfig>();

    /// <summary>
    /// 查询历史记录表
    /// </summary>
    public DbSet<QueryHistory> QueryHistories => Set<QueryHistory>();

    /// <summary>
    /// 查询参数表（关联到查询历史）
    /// </summary>
    public DbSet<QueryParameter> QueryParameters => Set<QueryParameter>();

    /// <summary>
    /// 应用全局设置表（仅一行记录）
    /// </summary>
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    /// <summary>
    /// 配置 SQLite 数据库连接，数据库文件存放在应用程序目录下的 data 文件夹中
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // 获取应用程序所在目录
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        var dataPath = Path.Combine(appDir, "data");

        // 确保 data 目录存在
        Directory.CreateDirectory(dataPath);

        var dbPath = Path.Combine(dataPath, "app.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    /// <summary>
    /// 配置实体映射关系和种子数据
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DatabaseConfig 配置
        modelBuilder.Entity<DatabaseConfig>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.DatabaseType)
                  .HasConversion<string>();
        });

        // FolderConfig 配置
        modelBuilder.Entity<FolderConfig>(entity =>
        {
            entity.HasIndex(e => e.Name);
        });

        // QueryHistory 配置
        modelBuilder.Entity<QueryHistory>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.LastUsedAt);
        });

        // QueryParameter 配置
        modelBuilder.Entity<QueryParameter>(entity =>
        {
            entity.HasIndex(e => e.QueryHistoryId);
        });

        // AppSettings 种子数据（确保始终有一行默认设置）
        modelBuilder.Entity<AppSettings>().HasData(new AppSettings { Id = 1 });
    }
}
