using System.ComponentModel.DataAnnotations;
using Sql2Json.Core.Enums;

namespace Sql2Json.Core.Models;

/// <summary>
/// 数据库连接配置，持久化到本地 SQLite
/// </summary>
public class DatabaseConfig
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 配置名称，用于 UI 显示（如"生产库-订单"）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 数据库类型
    /// </summary>
    public DatabaseType DatabaseType { get; set; }

    /// <summary>
    /// 连接字符串
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 命令执行超时时间（秒），0 表示不限时
    /// </summary>
    public int CommandTimeout { get; set; } = 300;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 排序权重，越小越靠前
    /// </summary>
    public int SortOrder { get; set; }
}
