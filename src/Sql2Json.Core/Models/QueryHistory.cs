using System.ComponentModel.DataAnnotations;

namespace Sql2Json.Core.Models;

/// <summary>
/// 查询历史记录，持久化到本地 SQLite，方便复用常用 SQL
/// </summary>
public class QueryHistory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 查询名称（用户自定义，方便识别）
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SQL 语句内容
    /// </summary>
    [Required]
    public string Sql { get; set; } = string.Empty;

    /// <summary>
    /// 关联的数据库配置 ID
    /// </summary>
    public Guid? DatabaseConfigId { get; set; }

    /// <summary>
    /// 参数列表（JSON 序列化存储）
    /// </summary>
    public string ParametersJson { get; set; } = "[]";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 使用次数
    /// </summary>
    public int UsageCount { get; set; }
}
