using System.ComponentModel.DataAnnotations;

namespace Sql2Json.Core.Models;

/// <summary>
/// 应用全局设置，持久化到本地 SQLite（单行记录）
/// </summary>
public class AppSettings
{
    [Key]
    public int Id { get; set; } = 1;

    /// <summary>
    /// JSON 导出是否格式化缩进
    /// </summary>
    public bool JsonIndented { get; set; } = true;

    /// <summary>
    /// JSON 导出是否使用 UTF-8 BOM
    /// </summary>
    public bool JsonUseBom { get; set; }

    /// <summary>
    /// JSON 缩进字符数
    /// </summary>
    public int JsonIndentSize { get; set; } = 2;

    /// <summary>
    /// JSON 属性名是否转驼峰
    /// </summary>
    public bool JsonCamelCase { get; set; }

    /// <summary>
    /// 任务最大并发执行数
    /// </summary>
    public int MaxConcurrency { get; set; } = 5;

    /// <summary>
    /// 结果预览最大行数
    /// </summary>
    public int PreviewMaxRows { get; set; } = 100;

    /// <summary>
    /// 是否使用暗色主题
    /// </summary>
    public bool IsDarkTheme { get; set; }

    /// <summary>
    /// 任务完成时是否发送通知（true=通知，false=静默）
    /// </summary>
    public bool EnableNotification { get; set; } = true;
}
