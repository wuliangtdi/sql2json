using Microsoft.EntityFrameworkCore;
using Sql2Json.Core.Data;
using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// 配置服务实现，通过 EF Core 操作本地 SQLite 数据库
/// 管理数据库配置、文件夹配置和应用全局设置
/// </summary>
public class ConfigService : IConfigService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public ConfigService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    // ===== 数据库配置 =====

    /// <inheritdoc/>
    public async Task<List<DatabaseConfig>> GetAllDatabaseConfigsAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.DatabaseConfigs
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<DatabaseConfig?> GetDatabaseConfigAsync(Guid id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.DatabaseConfigs.FindAsync(id);
    }

    /// <inheritdoc/>
    public async Task<DatabaseConfig> AddDatabaseConfigAsync(DatabaseConfig config)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.DatabaseConfigs.Add(config);
        await db.SaveChangesAsync();
        return config;
    }

    /// <inheritdoc/>
    public async Task UpdateDatabaseConfigAsync(DatabaseConfig config)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.DatabaseConfigs.Update(config);
        await db.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteDatabaseConfigAsync(Guid id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var config = await db.DatabaseConfigs.FindAsync(id);
        if (config != null)
        {
            db.DatabaseConfigs.Remove(config);
            await db.SaveChangesAsync();
        }
    }

    // ===== 文件夹配置 =====

    /// <inheritdoc/>
    public async Task<List<FolderConfig>> GetAllFolderConfigsAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.FolderConfigs
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<FolderConfig> AddFolderConfigAsync(FolderConfig config)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.FolderConfigs.Add(config);
        await db.SaveChangesAsync();
        return config;
    }

    /// <inheritdoc/>
    public async Task UpdateFolderConfigAsync(FolderConfig config)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.FolderConfigs.Update(config);
        await db.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteFolderConfigAsync(Guid id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var config = await db.FolderConfigs.FindAsync(id);
        if (config != null)
        {
            db.FolderConfigs.Remove(config);
            await db.SaveChangesAsync();
        }
    }

    // ===== 应用设置 =====

    /// <inheritdoc/>
    public async Task<AppSettings> GetAppSettingsAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        // 始终只有一行记录（通过种子数据保证）
        var settings = await db.AppSettings.FindAsync(1);
        return settings ?? new AppSettings();
    }

    /// <inheritdoc/>
    public async Task UpdateAppSettingsAsync(AppSettings settings)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        settings.Id = 1;
        db.AppSettings.Update(settings);
        await db.SaveChangesAsync();
    }
}
