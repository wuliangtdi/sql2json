using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// 配置服务接口，负责管理数据库配置、文件夹配置和应用设置的 CRUD 操作
/// </summary>
public interface IConfigService
{
    // ===== 数据库配置 =====

    /// <summary>
    /// 获取所有数据库配置
    /// </summary>
    Task<List<DatabaseConfig>> GetAllDatabaseConfigsAsync();

    /// <summary>
    /// 根据 ID 获取数据库配置
    /// </summary>
    Task<DatabaseConfig?> GetDatabaseConfigAsync(Guid id);

    /// <summary>
    /// 添加数据库配置
    /// </summary>
    Task<DatabaseConfig> AddDatabaseConfigAsync(DatabaseConfig config);

    /// <summary>
    /// 更新数据库配置
    /// </summary>
    Task UpdateDatabaseConfigAsync(DatabaseConfig config);

    /// <summary>
    /// 删除数据库配置
    /// </summary>
    Task DeleteDatabaseConfigAsync(Guid id);

    // ===== 文件夹配置 =====

    /// <summary>
    /// 获取所有文件夹配置
    /// </summary>
    Task<List<FolderConfig>> GetAllFolderConfigsAsync();

    /// <summary>
    /// 添加文件夹配置
    /// </summary>
    Task<FolderConfig> AddFolderConfigAsync(FolderConfig config);

    /// <summary>
    /// 更新文件夹配置
    /// </summary>
    Task UpdateFolderConfigAsync(FolderConfig config);

    /// <summary>
    /// 删除文件夹配置
    /// </summary>
    Task DeleteFolderConfigAsync(Guid id);

    // ===== 应用设置 =====

    /// <summary>
    /// 获取应用设置
    /// </summary>
    Task<AppSettings> GetAppSettingsAsync();

    /// <summary>
    /// 更新应用设置
    /// </summary>
    Task UpdateAppSettingsAsync(AppSettings settings);
}
