using System.ComponentModel.DataAnnotations;

namespace Sql2Json.Core.Models;

/// <summary>
/// 查询参数模型，用于参数化查询
/// </summary>
public class QueryParameter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 参数名（如 @id、@status）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 参数值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 所属查询历史 ID（可选，用于持久化历史参数）
    /// </summary>
    public Guid? QueryHistoryId { get; set; }
}
