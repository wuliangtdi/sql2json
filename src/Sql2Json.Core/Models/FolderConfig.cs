using System.ComponentModel.DataAnnotations;

namespace Sql2Json.Core.Models;

/// <summary>
/// 输出文件夹配置，持久化到本地 SQLite
/// </summary>
public class FolderConfig
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 文件夹显示名称（如"前端 Mock 数据"）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 文件夹绝对路径
    /// </summary>
    [Required]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 排序权重
    /// </summary>
    public int SortOrder { get; set; }
}
